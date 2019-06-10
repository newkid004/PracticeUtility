#pragma once
#include <functional>
#include <vector>
#include <memory>

#include <assert.h>

#include "CDelegateHandler.h"

template<typename ...Types>
class CDelegateHandler;

template<typename ...Types>
class CDelegate
{
public:
	using FuncArgs = void(Types...);
	using FuncType = std::function<void(Types...)>;

	using HandleType = CDelegateHandler<Types...>;

private:
	using SharedType = std::shared_ptr<HandleType>;
	using ContainType = std::unique_ptr<HandleType>;
	using ContainerType = std::vector<ContainType>;

private :
	ContainerType _Handlers;

private:
	void AddHandler(const FuncType& Event)
	{
		_Handlers.push_back(ContainType(new HandleType(Event)));
	}

	void AddHandler(const HandleType& Event)
	{
		_Handlers.push_back(ContainType(new HandleType(Event._Func)));
		_Handlers.back()->_ManagedID = (FuncType*)& Event._Func;
	}

	void DelHandler(const HandleType& Event)
	{
		for (int i = 0; i < (int)_Handlers.size(); ++i)
		{
			if (_Handlers[i]->_ManagedID == &Event._Func)
			{
				_Handlers.erase(_Handlers.begin() + i);
				return;
			}
		}
		assert(("Failed delete handler", false));
	}

public :
	SharedType CreateHandler(const FuncType& Event)
	{
		return SharedType(new HandleType(Event));
	}

	int ChainCount()
	{
		return (int)_Handlers.size();
	}

	void Clear()
	{
		_Handlers.clear();
	}

public:
	CDelegate& operator+=(const FuncType& Event)
	{
		AddHandler(Event);
		return *this;
	}

	CDelegate& operator+=(const SharedType& Event)
	{
		AddHandler(*Event);
		return *this;
	}

	CDelegate& operator-=(const SharedType& Event)
	{
		DelHandler(*Event);
		return *this;
	}

	void operator()(const Types& ... types)
	{
		for (auto & handle : _Handlers)
			handle->Invoke(types...);
	}

public :
	CDelegate() = default;
	virtual ~CDelegate() = default;
};
