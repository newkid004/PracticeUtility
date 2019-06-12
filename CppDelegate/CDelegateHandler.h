#pragma once

template<typename Return, typename ...Types>
class CDelegate;

template<typename Return, typename ...Types>
class CDelegateHandler
{
private :
	using OwnType = CDelegateHandler<Return, Types...>;

public:
	friend class CDelegate<Return, Types...>;

public :
	using FuncArgs = Return(Types...);
	using FuncType = std::function<Return(Types...)>;

private :
	FuncType* _ManagedID = nullptr;
	FuncType _Func;

public :
	Return Invoke(const Types&... types)
	{
		return _Func(types...);
	}

public :
	CDelegateHandler(const FuncType& Func) : _Func(Func) { };
	virtual ~CDelegateHandler() = default;
};

#pragma region CAction

template<typename ...Types>
class CDelegateHandler<void, Types...>
{
private:
	using OwnType = CDelegateHandler<void, Types...>;

public:
	friend class CDelegate<void, Types...>;

public:
	using FuncArgs = void(Types...);
	using FuncType = std::function<void(Types...)>;

private:
	FuncType* _ManagedID = nullptr;
	FuncType _Func;

public:
	void Invoke(const Types& ... types)
	{
		_Func(types...);
	}

public:
	CDelegateHandler(const FuncType& Func) : _Func(Func) { };
	virtual ~CDelegateHandler() = default;
};

template<>
class CDelegateHandler<void>
{
private:
	using OwnType = CDelegateHandler<void>;

public:
	friend class CDelegate<void>;

public:
	using FuncArgs = void();
	using FuncType = std::function<void()>;

private:
	FuncType* _ManagedID = nullptr;
	FuncType _Func;

public:
	void Invoke()
	{
		_Func();
	}

public:
	CDelegateHandler(const FuncType& Func) : _Func(Func) { };
	virtual ~CDelegateHandler() = default;
};

#pragma endregion
