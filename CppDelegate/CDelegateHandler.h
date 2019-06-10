#pragma once

template<typename ...Types>
class CDelegate;

template<typename ...Types>
class CDelegateHandler
{
private :
	using OwnType = CDelegateHandler<Types...>;

public:
	friend class CDelegate<Types...>;

public :
	using FuncArgs = void(Types...);
	using FuncType = std::function<void(Types...)>;

private :
	FuncType* _ManagedID = nullptr;
	FuncType _Func;

public :
	void Invoke(const Types&... types)
	{
		_Func(types...);
	}

public :
	CDelegateHandler(const FuncType& Func) : _Func(Func) { };
	virtual ~CDelegateHandler() = default;
};
