#include <cad_module.h>
#include <cad_object.h>
#include <queue>
#include <set>
#define IS_ONLY_ARROW_AND_NUMBER(X)	((X == (MAP_ARROW_LEFT | MAP_NUMBER) || (X == MAP_ARROW_RIGHT  | MAP_NUMBER) || (X == MAP_ARROW_DOWN  | MAP_NUMBER) || (X == MAP_ARROW_UP  | MAP_NUMBER))
cad_route_map *Open(cad_kernel *c, cad_route_map *m);
cad_route_map *Close(cad_kernel *c, cad_route_map *m);

cad_module_begin()
	set_module_name( "����� ����������� �����������. �������" ) // ��� ����� ������������ � ������ ��������� �������
	set_module_priority( 0 ) // ������ = 0
	set_module_capability( CAP_TRACEROUTE ) // ������ ��� ������ �����������
	set_module_callbacks( Open, Close ) // ������ ��� ������������� � ���������� ������. 
cad_module_end()

// ��������� ������. �� �������� ��������� ����������, � �������� �� ���. 
struct cad_route_map_private
{
	// ������ ���� ����� ��� ������ ���������� ����������
	cad_kernel *kernel;
	// ������� ������ � ������
	std::deque<uint64_t> queue;
	// ��������� ��� �� ����������������� ����������
	std::set<cad_wire *> NewWires;
	std::set<cad_wire *>::iterator CurrentWire;

	std::set<uint64_t> EndPoints;

	uint32_t current_start_x;
	uint32_t current_start_y;
};

// ������� ��������� ������, ������� ��� ������������ ������� �������.
// ����� ����� ��������� cad_route_map *self ������ �� ����� ������������ ���
// � �������� ����� �������������� ������ �������
uint32_t AboutToDestroy(cad_route_map *self)
{
	delete self->sys;
	self->sys = NULL;
	self->AboutToDestroy = NULL;
	self->MakeStepInDemoMode = NULL;
	return 0;
}

// �������� ��� ����� ����������
uint32_t Clear(cad_route_map *self)
{
	for (uint32_t i = 0; i < self->height * self->width * self->depth; i++)
	{
		if ((self->map[i] & 0xF0000000) == MAP_PIN) 
			self->map[i] &= 0xF0FFFFFF; else
			self->map[i]  =  MAP_EMPTY;
	}
	
	// �������������������� ���������� ���������
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

// ���� ����� �������� ��������
uint32_t MakeStepInDemoMode( cad_route_map *self)
{
	// ��� ������� ����� ���������� ��� ����, ��� �� ������� 1 ��� ��������������� �����. 
	// �������� ����� ��������: ��� ��������� ������ ���. ���� ����� ������ ��� �����������,
	// ����������� � ������������ (����������������), � ������� ������ �����.
	// ������ �������� - ��� ��� �� ������ ����� �������. ����� ����� ��� �����������.
	// ���� ������� ��������� ���. ���� � ���� ���� ��������� ������� � �������� ������, �� ���� ������ � �����
	// ��� ���������, �����, � ���������� ������ ��� ����� ���, ��� ��� ������.
	
	// ��� �� ��������, ��� � ���������� ���� ���� ��������� �����, � ����� ������ ����� ����� ��������� ������,
	// � ������ ����������� ����� �������. 
	// ��� ��� ���������� � 1 ����. ����� ����������� ���� �������� � 1 ���� ���������, ���� ���������, ������� 
	// �� ��������� ����������� ���� ��������. ���� ��, �� ������� ��� LAST_ACTION_OK. ���� ���,
	// �� ���� �������� ��� ���� ����, � ��������� ����������� ����������, ������� ������� �� ����� ����.
	// ���� � �� ��� ���� �� �� ����������, �� ������� ��� ����, � ��.
	// ��� �������� ���� ����� ������������ ������� self->ReallocMap. ��������� ����� ���������� �����.
	// ����� ���� ���������������� ���������� MAP_PIN � MAP_EMPTY � ��������������� �� ��������.
	// ��� �� ���� �������� �������� ��������� self->currerntLayer, ��� �� ������ ���������� ����, ����� ���� 
	// ����� ����������.

	// ����� ���� ��� ������ ��� �������������� �����, � �������� ����� �� ����������, ����� ������� 
	// MORE_ACTIONS_IN_DEMO_MODE
	// ���� ���������� �������� �����, � ��� ���� ��� �������� ������, � ���� ��� �� ����������� �������
	// ������� MORE_ACTIONS
	// ���� ���������, ��� ������ ������ ������, ������ LAST_ACTION_OK
	return MakeStepInDemoModeImplementation( self );
}


// � ���� ����� �� ����������. 
// �������� ������ ������ - ������� ���� ��������� cad_route_map_private
// � ���������������� ��������� �� ������� ������ ���������.
cad_route_map *Open(cad_kernel *c, cad_route_map *m)
{
	m->sys = new cad_route_map_private;
	m->sys->kernel = c;
	m->AboutToDestroy = AboutToDestroy;
	m->MakeStepInDemoMode = MakeStepInDemoMode; 
	m->Clear = Clear;
	m->Clear( m );
	return m;
}

// ��� ������ �� ���� ������
cad_route_map *Close(cad_kernel *c, cad_route_map *m)
{
	return NULL;
}

//----------------------------------------------------------------------------------//
//--------------���������� ����� ��������� (����� ������� ���������)----------------//
//----------------------------------------------------------------------------------//

// ��������������� ����������
uint32_t FindWireToTrace( cad_route_map *self );

// ������� ��������� �� �������
bool SetHalfLine(cad_route_map *self, uint32_t &i, uint32_t &j, uint32_t x, uint32_t y)
{
	uint32_t &val1 = MapElement3D(self, i, j, self->currerntLayer);
	uint32_t &val2 = MapElement3D(self, x, y, self->currerntLayer);
	self->sys->EndPoints.insert(((0LL + i) << 32) | j);
	self->sys->EndPoints.insert(((0LL + x) << 32) | y);

	if (val1 == (MAP_ARROW_DOWN | MAP_NUMBER)  ||	val1 == (MAP_ARROW_RIGHT | MAP_NUMBER)  ||
			val1 == (MAP_ARROW_LEFT | MAP_NUMBER)  ||	val1 == (MAP_ARROW_UP | MAP_NUMBER)	)	val1 = MAP_EMPTY;
	if (val2 == (MAP_ARROW_DOWN | MAP_NUMBER)  ||	val2 == (MAP_ARROW_RIGHT | MAP_NUMBER)  ||
			val2 == (MAP_ARROW_LEFT | MAP_NUMBER)  ||	val2 == (MAP_ARROW_UP | MAP_NUMBER)	)	val2 = MAP_EMPTY;

	if ( i < x ) val2 |=  MAP_WIRE_UP	, val1 |= MAP_WIRE_DOWN  	; else
	if ( x < i ) val2 |= MAP_WIRE_DOWN	, val1 |=  MAP_WIRE_UP	; else 
	if ( j < y ) val2 |= MAP_WIRE_LEFT	, val1 |= MAP_WIRE_RIGHT; else
	if ( y < j ) val2 |= MAP_WIRE_RIGHT	, val1 |= MAP_WIRE_LEFT	;

	i = x;
	j = y;
	return true;
}

// ���������� ��������� ������ �������� ������� �� ����������� ���������
bool SetHalfLine(cad_route_map *self, uint32_t &i, uint32_t &j, uint32_t &ArrowType)
{
	if ((ArrowType & MAP_ARROW_DOWN) == MAP_ARROW_DOWN)	
	{
		if ((self->sys->current_start_x == i+1 ) && (self->sys->current_start_y == j))
		{
			SetHalfLine(self, i, j, i+1, j   ); 
			return false; 
		}
		else return ArrowType = MapElement3D(self, i+1, j, self->currerntLayer), SetHalfLine(self, i, j, i+1, j   );
	}
	if ((ArrowType & MAP_ARROW_UP) == MAP_ARROW_UP)		
	{
		if ((self->sys->current_start_x == i-1 ) && (self->sys->current_start_y == j))
		{
			SetHalfLine(self, i, j, i-1, j   ); 
			return false; 
		}
		else return ArrowType = MapElement3D(self, i-1, j, self->currerntLayer), SetHalfLine(self, i, j, i-1, j   );
	}
	if ((ArrowType & MAP_ARROW_LEFT) == MAP_ARROW_LEFT)	
	{
		if ((self->sys->current_start_x == i ) && (self->sys->current_start_y == j-1))
		{
			SetHalfLine(self, i, j, i, j-1   ); 
			return false; 
		}
		return ArrowType = MapElement3D(self, i, j-1, self->currerntLayer), SetHalfLine(self, i, j, i  , j-1 );
	}
	if ((ArrowType & MAP_ARROW_RIGHT) == MAP_ARROW_RIGHT)	
	{
		if ((self->sys->current_start_x == i ) && (self->sys->current_start_y == j+1))
		{
			SetHalfLine(self, i, j, i, j+1   ); 
			return false; 
		}
		return ArrowType = MapElement3D(self, i, j+1, self->currerntLayer), SetHalfLine(self, i, j, i  , j+1 );
	}
	return false;
}

// �������� ������ ����� 2 ������� �� ����� (�� ����������)
void BuildLine(cad_route_map *self, uint32_t i, uint32_t j, uint32_t ArrowType)
{
	while ( SetHalfLine(self, i, j, ArrowType) );

	for(uint32_t i = 0; i < self->height; i++)
	{
		for(uint32_t j = 0; j < self->width; j++)
		{
			uint32_t &val = MapElement3D(self, i, j, self->currerntLayer);

			if ( (val & MAP_PIN) == MAP_PIN ) continue;

			if ( IS_HAS_ARROW(val) && IS_HAS_WIRE(val) )
			{
				val = val - (val & MAP_ARROW_LEFT) - (val & MAP_NUMBER) - (val & NUMBER_MASK);
				val = val - (val & MAP_ARROW_RIGHT) - (val & MAP_NUMBER) - (val & NUMBER_MASK);
				val = val - (val & MAP_ARROW_DOWN) - (val & MAP_NUMBER) - (val & NUMBER_MASK);
				val = val - (val & MAP_ARROW_UP) - (val & MAP_NUMBER) - (val & NUMBER_MASK);
				continue;
			}
			if ( IS_ONLY_ARROW(val) | (val & MAP_NUMBER))
			{
				val = MAP_EMPTY;
			}
		}
	}
}
	
// ��������� ��������� � ����� i j 
uint32_t SetPoint(cad_route_map *self, uint32_t i, uint32_t j, uint32_t ArrowType)
{
	if (i < 0 || j < 0 || i >= self->height || j >= self->width) return MORE_ACTIONS_IN_DEMO_MODE;
	if (i == self->sys->current_start_x && j == self->sys->current_start_y) return MORE_ACTIONS_IN_DEMO_MODE;

	if ( self->sys->EndPoints.empty() && 
		(MapElement3D(self, i, j, self->currerntLayer) == (MAP_PIN | (*self->sys->CurrentWire)->number)) ||
		self->sys->EndPoints.find(((0LL + i) << 32) | j) != self->sys->EndPoints.end())
	{
		// ����� �����. ��������� ������� ������������ � �������� � ����� i j
		int x = (MapElement3D(self, i, j, self->currerntLayer));	
		bool c = self->sys->EndPoints.empty() && 
		(MapElement3D(self, i, j, self->currerntLayer) == (MAP_PIN | (*self->sys->CurrentWire)->number));
		BuildLine(self, i,j, ArrowType);
		self->sys->queue.clear();
		return FindWireToTrace( self );
	}

	uint32_t val = MapElement3D(self, i, j, self->currerntLayer);
	uint32_t val2 = MapElement3D(self, i+1, j, self->currerntLayer);

	if ( val & MAP_PIN ) return MORE_ACTIONS_IN_DEMO_MODE;

	if ( val != MAP_EMPTY &&
		 val != MAP_WIRE_HORIZONTAL &&
		 val != MAP_WIRE_VERTICAL )
	{
		return MORE_ACTIONS_IN_DEMO_MODE;
	}
	
	self->sys->queue.push_back(((0LL + i) << 32) | j);

	if ( (val == MAP_WIRE_VERTICAL || val == MAP_WIRE_HORIZONTAL) )
	{
		uint32_t num = ArrowType & NUMBER_MASK;
		ArrowType =  (MAP_ARROW_RIGHT | MAP_NUMBER) | num;
		if (((val2 & MAP_WIRE_VERTICAL)==MAP_WIRE_VERTICAL) && ((val & MAP_WIRE_VERTICAL)==MAP_WIRE_VERTICAL))
		ArrowType += 0;
		else ArrowType += 1;
	}
	
	val |= (ArrowType | MAP_NUMBER); 
	MapElement3D(self, i, j, self->currerntLayer) = val;

	return MORE_ACTIONS_IN_DEMO_MODE;
}

// ������� 1 ��� ������ � ������
uint32_t ContinueWave( cad_route_map *self )
{
	uint32_t queuesize = self->sys->queue.size();

	uint32_t vrb;

	for (uint32_t _t = 0; _t < queuesize; _t++)
	{
		uint32_t i = (uint32_t)(self->sys->queue.front() >> 32);
		uint32_t j = self->sys->queue.front() &  ((1LL << 32) - 1);
		self->sys->queue.pop_front();

		uint32_t result ;

		uint32_t val = MapElement3D(self, i, j, self->currerntLayer);

		vrb = 0;

		if ( (val & MAP_NUMBER) == MAP_NUMBER )
		{
			vrb = val & NUMBER_MASK;
		}
		
		vrb |= MAP_NUMBER;

		result = SetPoint(self, i + 0, j + 1, MAP_ARROW_LEFT	| vrb ); if (result != MORE_ACTIONS_IN_DEMO_MODE) return result;
		result = SetPoint(self, i + 0, j - 1, MAP_ARROW_RIGHT	| vrb ); if (result != MORE_ACTIONS_IN_DEMO_MODE) return result;
		result = SetPoint(self, i + 1, j + 0, MAP_ARROW_UP		| vrb ); if (result != MORE_ACTIONS_IN_DEMO_MODE) return result;
		result = SetPoint(self, i - 1, j + 0, MAP_ARROW_DOWN	| vrb ); if (result != MORE_ACTIONS_IN_DEMO_MODE) return result;
	}
	return MORE_ACTIONS_IN_DEMO_MODE;
}

// ����������, � ����� ������� � �� ����� ���� ������ ������ �����������.
// ���� � �������� ������� ����� 2 ���������, �� ��� ����������� ���������� ��� �� ������������
// �������� ����� �������. 
uint32_t FindWireToTrace( cad_route_map *self )
{
	// ���������, ���� �� ��� �� ���������������� �������� � �������� �������
	for(uint32_t i = 0; i < self->height; i++)
		for(uint32_t j = 0; j < self->width; j++)
		{
			// ���� ��� �� ������� �������� �������, �� ����������
			if (MapElement3D(self, i,j, self->currerntLayer) != 
				(MAP_PIN | (*self->sys->CurrentWire)->number)) continue;

			// ���� ���� �������� ��� � �������� ��������� EndPoints, �� ����������
			if (self->sys->EndPoints.find(((0LL + i) << 32) | j) != self->sys->EndPoints.end()) continue;
			
			// �� �����. � ��� ������, ��� � ����� i j ����� ������ ����� � ������

			self->sys->queue.push_back(((0LL + i) << 32) | j);
			self->sys->current_start_x = i;
			self->sys->current_start_y = j;

			self->sys->kernel->PrintInfo("��� ����������� ������ ������ �%d, ������� (%d %d)",
				(*self->sys->CurrentWire)->number, i, j );
			return MORE_ACTIONS;
		}
	
	// ���, ������ ��� ������� ������ ��������� �������������. ��������� � ���������� �������.
	
	auto tmp = self->sys->CurrentWire;
	self->sys->CurrentWire++;
	self->sys->NewWires.erase( tmp );

	if (self->sys->NewWires.empty()) 
	{
		self->sys->kernel->PrintInfo("����������� ���������");
		return LAST_ACTION_OK;
	}

	// �������� ����� ����, ���� ������ �� ���� ��������, � ��� �� ��� ���������������
	if ( self->sys->CurrentWire == self->sys->NewWires.end())
	{	
		self->sys->CurrentWire = self->sys->NewWires.begin();
		self->ReallocMap(self, self->depth + 1);
		self->currerntLayer = self->depth - 1;
		self->sys->kernel->PrintInfo("�������� ����� ����");
	}
	
	// ������� ������, ������ �� ������������� ��� ����� �������.
	self->sys->EndPoints.clear();
	return FindWireToTrace( self );
}

uint32_t MakeStepInDemoModeImplementation( cad_route_map *self )
{
	uint32_t res = MORE_ACTIONS;

	if (self->sys->queue.empty())	
		res = FindWireToTrace(self);
	
	if (res == MORE_ACTIONS) res = ContinueWave( self );
	return res;
}