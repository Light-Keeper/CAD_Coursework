#include <cad_module.h>
#include <cad_object.h>
#include <queue>
#include <set>

cad_route_map *Open(cad_kernel *c, cad_route_map *m);
cad_route_map *Close(cad_kernel *c, cad_route_map *m);

cad_module_begin()
	set_module_name( "Метод путевых координат. Янушкевич" ) // это будет отображаться в списке доступных модулей
	set_module_priority( 0 ) // всегда = 0
	set_module_capability( CAP_TRACEROUTE ) // значит это модуль трассировки
	set_module_callbacks( Open, Close ) // методы для инициализации и отключения модуля. 
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
		if ((self->map[i] & MAP_PIN) != MAP_PIN) 
			self->map[i] = MAP_EMPTY;
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
//	return MakeStepInDemoModeImplementation( self );
	return LAST_ACTION_OK;
}


// у всех такая же реализация. 
// основная задача метода - создать вашу структуру cad_route_map_private
// и инициализировать указатели на функции вашими функциями.
cad_route_map *Open(cad_kernel *c, cad_route_map *m)
{
	m->sys = new cad_route_map_private;
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

// поставить стрелочку в точке i j 
uint32_t SetPoint(cad_route_map *self, uint32_t i, uint32_t j, uint32_t ArraowType)
{
	if (i < 0 || j < 0 || i >= self->height || j >= self->width) return MORE_ACTIONS_IN_DEMO_MODE;

	if ( self->sys->EndPoints.empty() && 
		(MapElement3D(self, i, j, self->currerntLayer) == (MAP_PIN | (*self->sys->CurrentWire)->number)) ||
		self->sys->EndPoints.find(((0LL + i) << 32) | j) != self->sys->EndPoints.end())
	{
		// конец волны. исходныая позиция подключается к конечной в точке i j
		//TODO: implement
	}

	if (MapElement3D(self, i, j, self->currerntLayer) != MAP_EMPTY) return MORE_ACTIONS_IN_DEMO_MODE;

	self->sys->queue.push_back(((0LL + i) << 32) | j);
	MapElement3D(self, i, j, self->currerntLayer) = ArraowType;
	return MORE_ACTIONS_IN_DEMO_MODE;
}

// сделать 1 шаг обхода в ширину
uint32_t ContinueWave( cad_route_map *self )
{
	uint32_t queuesize = self->sys->queue.size();
	
	for (uint32_t _t = 0; _t < queuesize; _t++)
	{
		uint32_t i = (uint32_t)(self->sys->queue.front() >> 32);
		uint32_t j = self->sys->queue.front() &  ((1LL << 32) - 1);

		uint32_t result ;
		result = SetPoint(self, i + 0, j + 1, MAP_ARROW_LEFT	); if (result != MORE_ACTIONS_IN_DEMO_MODE) return result;
		result = SetPoint(self, i + 0, j - 1, MAP_ARROW_RIGHT	); if (result != MORE_ACTIONS_IN_DEMO_MODE) return result;
		result = SetPoint(self, i + 1, j + 0, MAP_ARROW_UP		); if (result != MORE_ACTIONS_IN_DEMO_MODE) return result;
		result = SetPoint(self, i - 1, j + 0, MAP_ARROW_DOWN	); if (result != MORE_ACTIONS_IN_DEMO_MODE) return result;
	}
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
			self->sys->kernel->PrintInfo("Для трассировки выбран провод №%d, позиция (%d %d)",
				(*self->sys->CurrentWire)->number, i, j );
			return MORE_ACTIONS_IN_DEMO_MODE;
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
	return ((self->sys->queue.empty()) ? FindWireToTrace : ContinueWave) (self);
}