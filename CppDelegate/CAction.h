#pragma once

#include "CDelegate.h"

template<typename ...Type>
class CAction : public CDelegate<void, Type...>
{
public :
	CAction() = default;
	~CAction() = default;
};

template<>
class CAction<void> : public CDelegate<void>
{
public:
	CAction() = default;
	~CAction() = default;
};
