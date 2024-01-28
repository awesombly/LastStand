#include <Winsock2.h>

static const u_short HeaderSize  = 4;
static const u_short MaxDataSize = 2048;

using PacketType = u_short;

#define HEADER static const std::string name; static const PacketType type;
#define BODY ( _name ) const std::string _name::name = #_name; const PacketType _name::type = GetPacketType( _name::name.c_str() );

PacketType GetPacketType( const char* _name );