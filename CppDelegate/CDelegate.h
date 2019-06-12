#pragma once
#include <functional>
#include <vector>
#include <memory>

#include <assert.h>

#include "CDelegateHandler.h"

template<typename Return, typename ...Types>
class CDelegateHandler;

template<typename Return, typename ...Types>
class CDelegate
{
public:
	using FuncArgs = Return(Types...);
	using FuncType = std::function<Return(Types...)>;

	using HandleType = CDelegateHandler<Return, Types...>;

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
		auto iter = _Handlers.begin();

		while (iter != _Handlers.end())
		{
			if ((*iter)->_ManagedID == &Event._Func)
			{
				iter = _Handlers.erase(iter);
				return;
			}
			else
				++iter;
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

	template<typename Return, typename ...Types>
	Return operator()(const Types& ... types)
	{
		Return ReturnValue;

		for (auto & handle : _Handlers)
			ReturnValue = handle->Invoke(types...);

		return ReturnValue;
	}

public :
	CDelegate() = default;
	virtual ~CDelegate() = default;
};

#pragma region CAction

template<typename ...Types>
class CDelegate<void, Types...>
{
public:
	using FuncArgs = void(Types...);
	using FuncType = std::function<void(Types...)>;

	using HandleType = CDelegateHandler<void, Types...>;

private:
	using SharedType = std::shared_ptr<HandleType>;
	using ContainType = std::unique_ptr<HandleType>;
	using ContainerType = std::vector<ContainType>;

private:
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

public:
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
		for (auto& handle : _Handlers)
			handle->Invoke(types...);
	}

public:
	CDelegate() = default;
	virtual ~CDelegate() = default;
};

template<>
class CDelegate<void>
{
public:
	using FuncArgs = void();
	using FuncType = std::function<void()>;

	using HandleType = CDelegateHandler<void>;

private:
	using SharedType = std::shared_ptr<HandleType>;
	using ContainType = std::unique_ptr<HandleType>;
	using ContainerType = std::vector<ContainType>;

private:
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

public:
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

	void operator()()
	{
		for (auto& handle : _Handlers)
			handle->Invoke();
	}

public:
	CDelegate() = default;
	virtual ~CDelegate() = default;
};

#pragma endregion
