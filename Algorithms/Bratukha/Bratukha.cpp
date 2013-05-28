﻿#include <cad_module.h>
#include <cad_object.h>
#include <queue>
#include <set>

#define NO_MORE_ACTIONS			0x00000005

cad_route_map *Open(cad_kernel *c, cad_route_map *m);
cad_route_map *Close(cad_kernel *c, cad_route_map *m);

cad_module_begin()
	set_module_name( "Метод Аккерса. Братуха" )
	set_module_priority( 0 )
	set_module_capability( CAP_TRACEROUTE )
	set_module_callbacks(Open, Close)
cad_module_end()

// привытные данные. не заводите глобальне переменные, а хранивте всё тут. 
struct cad_route_map_private
{
	// объект ядра нужен для вывода отладочной информации
	cad_kernel *kernel;
	// очередь обхода в ширину
	std::deque<uint64_t> queue;
	// множество еще не протрассированных соединений
	std::set<cad_wire *> NewWires;
	std::set<cad_wire *>::iterator CurrentWire;

	std::set<uint64_t> EndPoints;

	uint32_t current_start_x;
	uint32_t current_start_y;
	uint8_t curr_num;
	uint8_t last_num;
};

// удалить привытные данные, закрыть все используемые модулем ресурсы.
// после этого структура cad_route_map *self больше не будет принадлежать вам
// и вероятно будет использоваться другим модулем
uint32_t AboutToDestroy(cad_route_map *self)
{
	delete self->sys;
	self->sys = NULL;
	self->AboutToDestroy = NULL;
	self->MakeStepInDemoMode = NULL;
	return 0;
}

// очистить все следы прибывания
uint32_t Clear(cad_route_map *self)
{
	for (uint32_t i = 0; i < self->height * self->width * self->depth; i++)
	{
		if ((self->map[i] & 0xF0000000) == MAP_PIN) 
			self->map[i] &= 0xF0FFFFFF; else
			self->map[i]  =  MAP_EMPTY;
	}
	
	// переинициализировать внутренние структуры
	self->sys->queue.clear();
	self->sys->NewWires.clear();
	
	for (uint32_t i = 0; i < self->sheme->connections.number_of_wires; i++)
		self->sys->NewWires.insert(&self->sheme->connections.wire[i]);
	
	self->sys->CurrentWire = self->sys->NewWires.begin();
	self->sys->EndPoints.clear();
	self->currerntLayer = 0;

	return 0;
}

uint32_t MakeStepInDemoModeImplementation( cad_route_map *self );

// этот метод реализут алгоритм
uint32_t MakeStepInDemoMode( cad_route_map *self)
{
	// эта функция будет вызываться для того, что бы сделать 1 шаг распространения волны. 
	// возможны такие ситуации: она вызвалась первый раз. надо найти провод для трассировки,
	// определится с приоритетеми (алгоримозависимо), и сделать первую волну.
	// другая ситуация - это уже не первый вызов функции. часть волны уже проставлена.
	// надо сделать следующий шаг. если в этом шаге произошла встреча с конечной точеой, то надо убрать с карты
	// все стрелочки, цифры, и проставить вместо них линию там, где она прошла.
	
	// так же возможно, что в предидущем шаге была проложена линия, в таком случае нужно найти следующий провод,
	// и начать трассировку этого провода. 
	// все это происходит в 1 слое. когда трассировка всех проводов в 1 слое закончена, надо проверить, удалось 
	// ли выполнить траммировку всех проводов. если да, то вернуть код LAST_ACTION_OK. если нет,
	// то надо добавить еще один слой, и выполнить трассировку соединений, которые отслись на новом слое.
	// если и на том слое не всё получилось, то создать еще один, и тд.
	// для создания слоя нужно использовать функцию self->ReallocMap. принимает новое количество слоев.
	// новые слои инициализируются значениями MAP_PIN и MAP_EMPTY в соответствующих им позициях.
	// так же надо изменить значение перемнной self->currerntLayer, что бы модуль прорисовки знал, какой слой 
	// нужно отображать.

	// после того как сделан шаг распростанения волны, и конечная точка не достигнута, нужно вернуть 
	// MORE_ACTIONS_IN_DEMO_MODE
	// если достигнута конечная точка, и при этом был проложен провод, и есть еще не проложенные провода
	// вернуть MORE_ACTIONS
	// если оказалось, что делать больше нечего, вернуь LAST_ACTION_OK
	return MakeStepInDemoModeImplementation( self );
}


// у всех такая же реализация. 
// основная задача метода - создать вашу структуру cad_route_map_private
// и инициализировать указатели на функции вашими функциями.
cad_route_map *Open(cad_kernel *c, cad_route_map *m)
{
	m->sys = new cad_route_map_private;
	m->sys->kernel = c;
	m->sys->last_num = 0;
	m->sys->curr_num = 0;
	m->AboutToDestroy = AboutToDestroy;
	m->MakeStepInDemoMode = MakeStepInDemoMode; 
	m->Clear = Clear;
	m->Clear( m );
	return m;
}

// тут ничего не надо делать
cad_route_map *Close(cad_kernel *c, cad_route_map *m)
{
	return NULL;
}

//----------------------------------------------------------------------------------//
//--------------реализация моего алгоритма (Метод путевых координат)----------------//
//----------------------------------------------------------------------------------//

// предварительное объявление
uint32_t FindWireToTrace( cad_route_map *self );

// заменят циферки на провода
bool SetHalfLine(cad_route_map *self, uint32_t &i, uint32_t &j, uint32_t x, uint32_t y)
{
	uint32_t &val1 = MapElement3D(self, i, j, self->currerntLayer);
	uint32_t &val2 = MapElement3D(self, x, y, self->currerntLayer);
	self->sys->EndPoints.insert(((0LL + i) << 32) | j);
	self->sys->EndPoints.insert(((0LL + x) << 32) | y);

	if ( val1 & MAP_NUMBER )
	{
		val1 = ((val1 | NUMBER_MASK) ^ NUMBER_MASK) ^ MAP_NUMBER;
	}

	if ( val2 & MAP_NUMBER )
	{
		val2 = ((val2 | NUMBER_MASK) ^ NUMBER_MASK) ^ MAP_NUMBER;
	}

	if ( i < x ) val2 |= MAP_WIRE_UP	, val1 |= MAP_WIRE_DOWN	; else
	if ( x < i ) val2 |= MAP_WIRE_DOWN	, val1 |= MAP_WIRE_UP	; else 
	if ( j < y ) val2 |= MAP_WIRE_LEFT	, val1 |= MAP_WIRE_RIGHT; else
	if ( y < j ) val2 |= MAP_WIRE_RIGHT	, val1 |= MAP_WIRE_LEFT	;

	i = x;
	j = y;

	return true;
}

void ChangeAccersNum( cad_route_map *self )
{
	if ( self->sys->curr_num == self->sys->last_num )
	{
		 self->sys->curr_num = self->sys->curr_num == 1 ? 0 : 1;
	}
	else
	{
		self->sys->last_num = self->sys->curr_num;
	}
}

// определяет коордианы нужной соседней клетики
bool SetHalfLine(cad_route_map *self, uint32_t &i, uint32_t &j, uint32_t &FillType	)
{
	uint32_t sx[] = {-1, +1, 0, 0}; // Смещение для просмотра элемента по X
	uint32_t sy[] = {0, 0, -1, +1}; // Смещение для просмотра элемента по Y

	uint32_t Point;

	if (FillType & MAP_NUMBER)	
	{
		for ( uint8_t k = 0; k < 4; k++ )
		{
			Point = MapElement3D( self, i + sx[k], j + sy[k], self->currerntLayer );

			if ( (Point & MAP_NUMBER) && ((Point & NUMBER_MASK) == self->sys->curr_num) ) 
			{
				ChangeAccersNum( self );
				return SetHalfLine(self, i, j, i + sx[k], j + sy[k]);
			}
			else if ( self->sys->current_start_x == i + sx[k] && self->sys->current_start_y == j + sy[k] )
			{
				SetHalfLine(self, i, j, i + sx[k], j + sy[k]);
				return false;
			}
		}
	}

	return false;
}

// провести провод между 2 точками на плате (по стрелочкам)
void BuildLine(cad_route_map *self, uint32_t i, uint32_t j, uint32_t FillType)
{
	// swap
	self->sys->last_num ^= self->sys->curr_num;
	self->sys->curr_num ^= self->sys->last_num;
	self->sys->last_num ^= self->sys->curr_num;

	while ( SetHalfLine(self, i, j, FillType) );

	for( uint32_t i = 0; i < self->height; i++ )
	{
		for( uint32_t j = 0; j < self->width; j++ )
		{
			uint32_t &val = MapElement3D( self, i, j, self->currerntLayer );

			if ( val & MAP_NUMBER )
			{
				val = MAP_EMPTY;
			}
		}
	}
}

// поставить циферку в точке i j 
uint32_t SetPoint(cad_route_map *self, uint32_t i, uint32_t j, uint32_t FillType)
{
	if (i < 0 || j < 0 || i >= self->height || j >= self->width) return MORE_ACTIONS_IN_DEMO_MODE;
	if (i == self->sys->current_start_x && j == self->sys->current_start_y) return MORE_ACTIONS_IN_DEMO_MODE;

	if ( self->sys->EndPoints.empty() && 
		(MapElement3D(self, i, j, self->currerntLayer) == (MAP_PIN | (*self->sys->CurrentWire)->number)) ||
		self->sys->EndPoints.find(((0LL + i) << 32) | j) != self->sys->EndPoints.end())
	{
		// конец волны. исходная позиция подключается к конечной в точке i j
		int x = MapElement3D(self, i, j, self->currerntLayer);
		bool c = self->sys->EndPoints.empty() && 
		(MapElement3D(self, i, j, self->currerntLayer) == (MAP_PIN | (*self->sys->CurrentWire)->number));
		BuildLine(self, i,j, FillType);
		self->sys->queue.clear();
		return FindWireToTrace( self );
	}

	uint32_t &val = MapElement3D(self, i, j, self->currerntLayer);

	if ( val != MAP_EMPTY && val != MAP_PIN ) return NO_MORE_ACTIONS;

	if ( val != MAP_EMPTY ) return MORE_ACTIONS_IN_DEMO_MODE;

	self->sys->queue.push_back(((0LL + i) << 32) | j);
	MapElement3D(self, i, j, self->currerntLayer) = FillType;
	return MORE_ACTIONS_IN_DEMO_MODE;
}

// сделать 1 шаг обхода в ширину
uint32_t ContinueWave( cad_route_map *self )
{
	uint32_t queuesize = self->sys->queue.size();

	bool no_more_actions = true;

	for (uint32_t _t = 0; _t < queuesize; _t++)
	{
		uint32_t i = (uint32_t)(self->sys->queue.front() >> 32);
		uint32_t j = self->sys->queue.front() &  ((1LL << 32) - 1);
		self->sys->queue.pop_front();

		uint32_t result ;
		uint32_t distance = MAP_NUMBER | self->sys->curr_num;

		result = SetPoint( self, i + 0, j + 1, distance );
		if ( no_more_actions && result != NO_MORE_ACTIONS )
		{
			no_more_actions = false;
		}
		if ( result != MORE_ACTIONS_IN_DEMO_MODE && result != NO_MORE_ACTIONS ) return result;

		result = SetPoint( self, i + 0, j - 1, distance );
		if ( no_more_actions && result != NO_MORE_ACTIONS )
		{
			no_more_actions = false;
		}
		if ( result != MORE_ACTIONS_IN_DEMO_MODE && result != NO_MORE_ACTIONS ) return result;

		result = SetPoint( self, i + 1, j + 0, distance );
		if ( no_more_actions && result != NO_MORE_ACTIONS )
		{
			no_more_actions = false;
		}
		if ( result != MORE_ACTIONS_IN_DEMO_MODE && result != NO_MORE_ACTIONS ) return result;

		result = SetPoint( self, i - 1, j + 0, distance );
		if ( no_more_actions && result != NO_MORE_ACTIONS )
		{
			no_more_actions = false;
		}
		if ( result != MORE_ACTIONS_IN_DEMO_MODE && result != NO_MORE_ACTIONS ) return result;
	}

	if ( no_more_actions )
	{
		return NO_MORE_ACTIONS;
	}

	ChangeAccersNum(self);
	return MORE_ACTIONS_IN_DEMO_MODE;
}

// определяет, с какой позиции и на каком слое сейчас начать трассировку.
// если у текущего провода более 2 контактов, то для трассировки выбирается еще не подключенные
// контакты этого провода. 
uint32_t FindWireToTrace( cad_route_map *self )
{
	// проверить, есть ли еще не оттрассированные контакты у текущего провода
	for(uint32_t i = 0; i < self->height; i++)
		for(uint32_t j = 0; j < self->width; j++)
		{
			// если это не контакт текущего провода, то пропустить
			if (MapElement3D(self, i,j, self->currerntLayer) != 
				(MAP_PIN | (*self->sys->CurrentWire)->number)) continue;

			// если этот контакте уже в найденом множестве EndPoints, то пропустить
			if (self->sys->EndPoints.find(((0LL + i) << 32) | j) != self->sys->EndPoints.end()) continue;
			
			// мы здесь. а это значит, что с точки i j можно начать обход в ширину

			self->sys->queue.push_back(((0LL + i) << 32) | j);
			self->sys->current_start_x = i;
			self->sys->current_start_y = j;

			self->sys->kernel->PrintInfo("Для трассировки выбран провод №%d, позиция (%d %d)",
				(*self->sys->CurrentWire)->number, i, j );
			return MORE_ACTIONS;
		}
	
	// тут, похоже что текущий провод полностью оттрассирован. переходим к следующему проводу.
	
	auto tmp = self->sys->CurrentWire;
	self->sys->CurrentWire++;
	self->sys->NewWires.erase( tmp );

	if (self->sys->NewWires.empty()) 
	{
		self->sys->kernel->PrintInfo("Трассировка закончена");
		return LAST_ACTION_OK;
	}

	// выделить новый слой, если прошли по всем проводам, а еще не все оттрассированны
	if ( self->sys->CurrentWire == self->sys->NewWires.end())
	{	
		self->sys->CurrentWire = self->sys->NewWires.begin();
		self->ReallocMap(self, self->depth + 1);
		self->currerntLayer = self->depth - 1;
		self->sys->kernel->PrintInfo("Добавлен новый слой");
	}
	
	// выбрали провод, делаем всё вышеописанное для этого провода.
	self->sys->EndPoints.clear();
	return FindWireToTrace( self );
}

uint32_t MakeStepInDemoModeImplementation( cad_route_map *self )
{
	uint32_t res = MORE_ACTIONS;

	if ( self->sys->queue.empty() )
		res = FindWireToTrace( self );
	
	if ( res == MORE_ACTIONS ) 
		res = ContinueWave( self );

	if ( res == NO_MORE_ACTIONS )
	{
		self->sys->CurrentWire = self->sys->NewWires.begin();
		self->ReallocMap(self, self->depth + 1);
		self->currerntLayer = self->depth - 1;
		self->sys->kernel->PrintInfo( "Добавлен новый слой №%d", self->currerntLayer );
		return MORE_ACTIONS;
	}

	return res;
}