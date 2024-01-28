#pragma once
// Warning Ignore
#define _CRT_SECURE_NO_WARNINGS
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#pragma warning( disable : 4505 ) // 사용 안하는 함수 제거

// Windows
#include <winsock2.h>
#include <WS2tcpip.h>
#include <windows.h>
#include <process.h>

// Character
#include <string>
#include <tchar.h>
#include <fstream>
#include <sstream>

// Data Structure
#include <vector>
#include <list>
#include <queue>
#include <map>

// Standard
#include <iostream>
#include <cstdio>
#include <random>

// Thread
#include <thread>
#include <mutex>
#include <future>
#include <type_traits>

// Function
#include <condition_variable>
#include <functional>

// Time
#include <chrono>
#include <iomanip>
#include <ctime>