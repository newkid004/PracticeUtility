using System;
using System.Collections.Generic;

public class BitFlag
{
	// Ref variant
	protected const int BitSize = 32;

	// Member variant
	protected List<int> _Container;
	protected bool _isBinary;

	// Indexer
	public virtual int this[int i]
	{
		get { return (_Container[getSafeContainIndex(i)] & getMask(i)) >> getInterval(i); }
		set
		{
			int iContain = getSafeContainIndex(i);
			_Container[iContain] = (_Container[iContain] & (~getMask(i))) | ((value & MaskNumber) << getInterval(i));
		}
	}

	// Functional property
	private int _nMaskCount = 1;
	protected int nMaskCount
	{
		get { return _nMaskCount; }
		set
		{
			RebuildContainer(value);

			_nMaskCount = value;
			_isBinary = CheckBinary(value);

			_MaskNumber = (1 << _nMaskCount) - 1;
			_OneContain = BitSize / _nMaskCount;
		}
	}

	// Variable property
	private int _MaskNumber = 1;
	public int MaskNumber { get { return _MaskNumber; } }

	private int _OneContain = BitSize;
	protected int OneContain { get { return _OneContain; } }

	// Constructor
	public BitFlag(int nMaskCount)
	{
		if (nMaskCount == 0)
			throw new DivideByZeroException();

		this._Container = new List<int>();
		this.nMaskCount = nMaskCount;
	}

	// Member method
	private bool CheckBinary(int value)
	{
		if (value <= 0)
			throw new Exception("Not implemented smaller than 0");

		if (BitSize < value)
			throw new Exception("Not implemented bigger than 32");

		bool isBin = true;
		for (int i = 1; i <= BitSize; i <<= 1)
		{
			if (value == i)
				break;

			else if (0 != (value % i))
			{
				isBin = false;
				break;
			}
		}

		if (!isBin)
			throw new Exception("Not implemented other than binary");

		return isBin;
	}

	private void RebuildContainer(int nNewMaskCount)
	{
		List<int> listOldValue = new List<int>(_Container);
		int nOldMask = (1 << _nMaskCount) - 1;
		int nNewMask = (1 << nNewMaskCount) - 1;

		int nOldBitContain = BitSize / _nMaskCount;
		int nNewBitContain = BitSize / nNewMaskCount;

		_Container.Clear();

		int iCurrent = 0;
		bool isNewBinary = CheckBinary(nNewMaskCount);

		for (int i = 0; i < listOldValue.Count; ++i)
		{
			while ((_nMaskCount * iCurrent) < (BitSize << i))
			{
				int nOldBitIndex = _nMaskCount * (iCurrent % nOldBitContain);
				int nOldValue = (listOldValue[i] & (nOldMask << nOldBitIndex)) >> nOldBitIndex;

				int nNewContainIndex = (iCurrent * (nNewBitContain)) / BitSize;
				int nNewBitIndex = nNewMaskCount * (iCurrent % nNewBitContain);
				int nNewValue = nOldValue & nNewMask;

				while (_Container.Count <= nNewContainIndex)
					_Container.Add(0);

				_Container[nNewContainIndex] |= nNewValue << nNewBitIndex;

				++iCurrent;
			}
		}
	}

	// Utility method
	public int getInterval(int nInterval)
	{
		return (nInterval % OneContain) * nMaskCount;
	}

	public int getMask(int nInterval)
	{
		return MaskNumber << getInterval(nInterval);
	}

	private int getSafeContainIndex(int i)
	{
		int iContain = i / OneContain;

		while (_Container.Count <= iContain)
			_Container.Add(0);

		return iContain;
	}

	public void Clear()
	{
		_Container.Clear();
	}

	/// <summary>
	/// Same as '=' operator
	/// </summary>
	public void bitSet(int i, int value)
	{
		int iContain = getSafeContainIndex(i);
		int iMask = getMask(i);

		_Container[iContain] = (_Container[iContain] & (~iMask)) | ((value & MaskNumber) << getInterval(i));
	}

	/// <summary>
	/// Same as '|' operator
	/// </summary>
	public void bitPut(int i, int value)
	{
		int iContain = getSafeContainIndex(i);
		_Container[iContain] = _Container[iContain] | ((value & MaskNumber) << getInterval(i));
	}

	/// <summary>
	/// Same as '&' operator
	/// </summary>
	public void bitAnd(int i, int value)
	{
		int iContain = getSafeContainIndex(i);
		int ninterval = getInterval(i);
		_Container[iContain] = _Container[iContain] & (((value & MaskNumber) << ninterval) | ~(MaskNumber << ninterval));
	}

	/// <summary>
	/// Same as 'value = 0'
	/// </summary>
	public void bitPick(int i)
	{
		int iContain = getSafeContainIndex(i);
		_Container[iContain] = _Container[iContain] & (~getMask(i));
	}

	/// <summary>
	/// Same as '^' operator
	/// </summary>
	public void bitToggle(int i, int value)
	{
		int iContain = getSafeContainIndex(i);
		_Container[iContain] = _Container[iContain] ^ ((value & MaskNumber) << getInterval(i));
	}

	public int GetContainNumber(int index)
	{
		if (_Container.Count <= index)
			return 0;

		return _Container[index];
	}
}
