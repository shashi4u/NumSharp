﻿/*
 * NumSharp
 * Copyright (C) 2018 Haiping Chen
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the Apache License 2.0 as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the Apache License 2.0
 * along with this program.  If not, see <http://www.apache.org/licenses/LICENSE-2.0/>.
 */

//TODO! Complete all TODOs return(?:.*;|;)[\n\r]{1,2}[\t\s]+//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using NumSharp.Backends;
using NumSharp.Backends.Unmanaged;
using NumSharp.Generic;
using NumSharp.Utilities;

namespace NumSharp
{
    /// <summary>
    ///     An array object represents a multidimensional, homogeneous array of fixed-size items.<br></br>
    ///     An associated data-type object describes the format of each element in the array (its byte-order,<br></br>
    ///     how many bytes it occupies in memory, whether it is an integer, a floating point number, or something else, etc.)
    /// </summary>
    /// <remarks>https://docs.scipy.org/doc/numpy/reference/generated/numpy.ndarray.html</remarks>
    [DebuggerTypeProxy(nameof(NDArrayDebuggerProxy))]
    public partial class NDArray : ICloneable, IEnumerable
    {
        protected TensorEngine tensorEngine;

        #region Constructors

        /// <summary>
        ///     Creates a new <see cref="NDArray"/> with this storage.
        /// </summary>
        /// <param name="storage"></param>
        internal NDArray(UnmanagedStorage storage)
        {
            Storage = storage;
            tensorEngine = storage.Engine;
        }

        /// <summary>
        ///     Creates a new <see cref="NDArray"/> with this storage.
        /// </summary>
        /// <param name="storage"></param>
        internal NDArray(UnmanagedStorage storage, Shape shape)
        {
            Storage = storage;
            storage.SetShapeUnsafe(shape);
            tensorEngine = storage.Engine;
        }

        /// <summary>
        ///     Creates a new <see cref="NDArray"/> with this storage.
        /// </summary>
        /// <param name="storage"></param>
        internal NDArray(UnmanagedStorage storage, ref Shape shape)
        {
            Storage = new UnmanagedStorage(storage.InternalArray, shape);
            tensorEngine = storage.Engine;
        }

        /// <summary>
        /// Constructor for init data type
        /// internal storage is 1D with 1 element
        /// </summary>
        /// <param name="dtype">Data type of elements</param>
        /// <param name="engine">The engine of this <see cref="NDArray"/></param>
        /// <remarks>This constructor does not call allocation/></remarks>
        internal NDArray(Type dtype, TensorEngine engine)
        {
            tensorEngine = engine;
            Storage = TensorEngine.GetStorage(dtype);
        }

        /// <summary>
        /// Constructor for init data type
        /// internal storage is 1D with 1 element
        /// </summary>
        /// <param name="typeCode">Data type of elements</param>
        /// <param name="engine">The engine of this <see cref="NDArray"/></param>
        /// <remarks>This constructor does not call allocation/></remarks>
        internal NDArray(NPTypeCode typeCode, TensorEngine engine)
        {
            tensorEngine = engine;
            Storage = TensorEngine.GetStorage(typeCode);
        }

        /// <summary>
        /// Constructor for init data type
        /// internal storage is 1D with 1 element
        /// </summary>
        /// <param name="dtype">Data type of elements</param>
        /// <remarks>This constructor does not call allocation/></remarks>
        public NDArray(Type dtype) : this(dtype, BackendFactory.GetEngine()) { }

        /// <summary>
        /// Constructor for init data type
        /// internal storage is 1D with 1 element
        /// </summary>
        /// <param name="typeCode">Data type of elements</param>
        /// <remarks>This constructor does not call allocation/></remarks>
        public NDArray(NPTypeCode typeCode) : this(typeCode, BackendFactory.GetEngine()) { }

        /// <summary>
        /// Constructor which takes .NET array
        /// dtype and shape is determined from array
        /// </summary>
        /// <param name="values"></param>
        /// <param name="shape"></param>
        /// <param name="order"></param>
        /// <returns>Array with values</returns>
        /// <remarks>This constructor calls <see cref="IStorage.Allocate(NumSharp.Shape,System.Type)"/></remarks>
        public NDArray(Array values, Shape shape = default, char order = 'C') : this(values.GetType().GetElementType())
        {
            if (order != 'C')
                shape.ChangeTensorLayout(order);
            Storage.Allocate(values, shape);
        }

        /// <summary>
        /// Constructor which takes .NET array
        /// dtype and shape is determined from array
        /// </summary>
        /// <param name="values"></param>
        /// <param name="shape"></param>
        /// <param name="order"></param>
        /// <returns>Array with values</returns>
        /// <remarks>This constructor calls <see cref="IStorage.Allocate(NumSharp.Shape,System.Type)"/></remarks>
        public NDArray(IArraySlice values, Shape shape = default, char order = 'C') : this(values.TypeCode)
        {
            if (order != 'C')
                shape.ChangeTensorLayout(order);
            Storage.Allocate(values, shape);
        }

        /// <summary>
        /// Constructor which initialize elements with 0
        /// type and shape are given.
        /// </summary>
        /// <param name="dtype">internal data type</param>
        /// <param name="shape">Shape of NDArray</param>
        /// <remarks>This constructor calls <see cref="IStorage.Allocate(NumSharp.Shape,System.Type)"/></remarks>
        public NDArray(Type dtype, Shape shape) : this(dtype, shape, true) { }

        /// <summary>
        ///     Constructor which initialize elements with length of <paramref name="size"/>
        /// </summary>
        /// <param name="dtype">Internal data type</param>
        /// <param name="size">The size as a single dimension shape</param>
        /// <remarks>This constructor calls <see cref="IStorage.Allocate(NumSharp.Shape,System.Type)"/></remarks>
        public NDArray(Type dtype, int size) : this(dtype, Shape.Vector(size), true) { }

        /// <summary>
        /// Constructor which initialize elements with 0
        /// type and shape are given.
        /// </summary>
        /// <param name="dtype">internal data type</param>
        /// <param name="shape">Shape of NDArray</param>
        /// <remarks>This constructor calls <see cref="IStorage.Allocate(NumSharp.Shape,System.Type)"/></remarks>
        public NDArray(NPTypeCode dtype, Shape shape) : this(dtype, shape, true) { }

        /// <summary>
        ///     Constructor which initialize elements with length of <paramref name="size"/>
        /// </summary>
        /// <param name="dtype">Internal data type</param>
        /// <param name="size">The size as a single dimension shape</param>
        /// <remarks>This constructor calls <see cref="IStorage.Allocate(NumSharp.Shape,System.Type)"/></remarks>
        public NDArray(NPTypeCode dtype, int size) : this(dtype, Shape.Vector(size), true) { }

        /// <summary>
        /// Constructor which initialize elements with 0
        /// type and shape are given.
        /// </summary>
        /// <param name="dtype">internal data type</param>
        /// <param name="shape">Shape of NDArray</param>
        /// <param name="fillZeros">Should set the values of the new allocation to default(dtype)? otherwise - old memory noise</param>
        /// <remarks>This constructor calls <see cref="IStorage.Allocate(NumSharp.Shape,System.Type)"/></remarks>
        public NDArray(Type dtype, Shape shape, bool fillZeros) : this(dtype)
        {
            Storage.Allocate(shape, dtype, fillZeros);
        }

        /// <summary>
        /// Constructor which initialize elements with 0
        /// type and shape are given.
        /// </summary>
        /// <param name="dtype">internal data type</param>
        /// <param name="shape">Shape of NDArray</param>
        /// <param name="fillZeros">Should set the values of the new allocation to default(dtype)? otherwise - old memory noise</param>
        /// <remarks>This constructor calls <see cref="IStorage.Allocate(NumSharp.Shape,System.Type)"/></remarks>
        public NDArray(NPTypeCode dtype, Shape shape, bool fillZeros) : this(dtype)
        {
            Storage.Allocate(shape, dtype, fillZeros);
        }

        private NDArray(IArraySlice array, Shape shape) : this(array.TypeCode)
        {
            Storage.Allocate(array, shape);
        }

        #endregion

        /// <summary>
        /// Data type of NDArray
        /// </summary>
        public Type dtype => Storage.DType;

        /// <summary>
        ///     Gets the precomputed <see cref="NPTypeCode"/> of <see cref="dtype"/>.
        /// </summary>
        internal NPTypeCode GetTypeCode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Storage.TypeCode;
        }

        /// <summary>
        ///     Gets the address that this NDArray starts from.
        /// </summary>
        internal unsafe void* Address
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Storage.Address;
        }

        /// <summary>
        /// Data length of every dimension
        /// </summary>
        public int[] shape => Storage.Shape.Dimensions;

        /// <summary>
        /// Dimension count
        /// </summary>
        public int ndim => Storage.Shape.NDim;

        /// <summary>
        /// Total of elements
        /// </summary>
        public int size => Storage.Shape.Size;

        public int dtypesize => Storage.DTypeSize;

        public char order => Storage.Shape.Order;

        public int[] strides => Storage.Shape.Strides;

        internal Shape Shape
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Storage.Shape;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Storage.Reshape(value);
        }

        /// <summary>
        /// The internal storage that stores data for this <see cref="NDArray"/>.
        /// </summary>
        internal UnmanagedStorage Storage;


        /// <summary>
        ///     The tensor engine that handles this <see cref="NDArray"/>.
        /// </summary>
        public TensorEngine TensorEngine
        {
            get => tensorEngine ?? Storage.Engine ?? BackendFactory.GetEngine();
            set => tensorEngine = (value ?? Storage.Engine ?? BackendFactory.GetEngine());
        }

        /// <summary>
        /// Shortcut for access internal elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ArraySlice<T> Data<T>() where T : unmanaged
        {
            return Storage.GetData<T>();
        }
        
        /// <summary>
        ///     Set a <see cref="IArraySlice"/> at given <see cref="indices"/>.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="indices">The </param>
        /// <remarks>
        ///     Does not change internal storage data type.<br></br>
        ///     If <paramref name="value"/> does not match <see cref="DType"/>, <paramref name="value"/> will be converted.
        /// </remarks>
        public void SetData(IArraySlice value, params int[] indices)
        {
            Storage.SetData(value, indices);
        }

        /// <summary>
        ///     Set a <see cref="NDArray"/> at given <see cref="indices"/>.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="indices">The </param>
        /// <remarks>
        ///     Does not change internal storage data type.<br></br>
        ///     If <paramref name="value"/> does not match <see cref="DType"/>, <paramref name="value"/> will be converted.
        /// </remarks>
        public void SetData(NDArray value, params int[] indices)
        {
            Storage.SetData(value, indices);
        }

        /// <summary>
        ///     Set a single value at given <see cref="indices"/>.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="indices">The </param>
        /// <remarks>
        ///     Does not change internal storage data type.<br></br>
        ///     If <paramref name="value"/> does not match <see cref="DType"/>, <paramref name="value"/> will be converted.
        /// </remarks>
        public void SetData(object value, params int[] indices)
        {
            Storage.SetData(value, indices);
        }

        /// <summary>
        ///     Set a single value at given <see cref="indices"/>.
        /// </summary>
        /// <param name="value">The value to set</param>
        /// <param name="indices">The </param>
        /// <remarks>
        ///     Does not change internal storage data type.<br></br>
        ///     If <paramref name="value"/> does not match <see cref="DType"/>, <paramref name="value"/> will be converted.
        /// </remarks>
        public unsafe void SetData<T>(T value, params int[] indices) where T : unmanaged
        {
            Storage.SetData<T>(value, indices);
        }

        /// <summary>
        ///     Sets <see cref="values"/> as the internal data storage and changes the internal storage data type to <see cref="dtype"/> and casts <see cref="values"/> if necessary.
        /// </summary>
        /// <param name="values">The values to set as internal data soruce</param>
        /// <param name="dtype">The type to change this storage to and the type to cast <see cref="values"/> if necessary.</param>
        /// <remarks>Does not copy values unless cast is necessary.</remarks>
        // ReSharper disable once ParameterHidesMember
        public void ReplaceData(Array values, Type dtype)
        {
            Storage.ReplaceData(values, dtype);
        }

        /// <summary>
        ///     Sets <see cref="values"/> as the internal data storage and changes the internal storage data type to <see cref="values"/> type.
        /// </summary>
        /// <param name="values"></param>
        /// <remarks>Does not copy values.</remarks>
        public void ReplaceData(Array values)
        {
            Storage.ReplaceData(values);
        }

        /// <summary>
        ///     Sets <see cref="nd"/> as the internal data storage and changes the internal storage data type to <see cref="nd"/> type.
        /// </summary>
        /// <param name="nd"></param>
        /// <remarks>Does not copy values and does change shape and dtype.</remarks>
        public void ReplaceData(NDArray nd)
        {
            Storage.ReplaceData(nd);
        }

        /// <summary>
        ///     Set an Array to internal storage, cast it to new dtype and if necessary change dtype  
        /// </summary>
        /// <param name="values"></param>
        /// <param name="typeCode"></param>
        /// <remarks>Does not copy values unless cast is necessary and doesn't change shape.</remarks>
        public void ReplaceData(Array values, NPTypeCode typeCode)
        {
            Storage.ReplaceData(values, typeCode);
        }

        /// <summary>
        ///     Sets <see cref="values"/> as the internal data source and changes the internal storage data type to <see cref="values"/> type.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="dtype"></param>
        /// <remarks>Does not copy values and doesn't change shape.</remarks>
        public void ReplaceData(IArraySlice values, Type dtype)
        {
            Storage.ReplaceData(values, dtype);
        }

        /// <summary>
        ///     Sets <see cref="values"/> as the internal data source and changes the internal storage data type to <see cref="values"/> type.
        /// </summary>
        /// <param name="values"></param>
        /// <remarks>Does not copy values and doesn't change shape.</remarks>
        public void ReplaceData(IArraySlice values)
        {
            Storage.ReplaceData(values);
        }

        /// <summary>
        ///     Gets the internal storage and converts it to <typeparamref name="T"/> if necessary.
        /// </summary>
        /// <typeparam name="T">The returned type.</typeparam>
        /// <returns>An array of type <typeparamref name="T"/></returns>
        public ArraySlice<T> GetData<T>() where T : unmanaged
        {
            return Storage.GetData<T>();
        }

        /// <summary>
        ///     Get reference to internal data storage
        /// </summary>
        /// <returns>reference to internal storage as System.Array</returns>
        public IArraySlice GetData()
        {
            return Storage.GetData();
        }

        /// <summary>
        ///     Get: Gets internal storage array by calling <see cref="IStorage.GetData"/><br></br>
        ///     Set: Replace internal storage by calling <see cref="IStorage.ReplaceData(System.Array)"/>
        /// </summary>
        /// <remarks>Setting does not replace internal storage array.</remarks>
        internal IArraySlice Array
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Storage.InternalArray;
        }

        public ArraySlice<T> CloneData<T>() where T : unmanaged => Storage.CloneData<T>();

        public IArraySlice CloneData() => Storage.CloneData();

        /// <summary>
        ///     Copy of the array, cast to a specified type.
        /// </summary>
        /// <param name="dtype">The dtype to cast this array.</param>
        /// <param name="copy">By default, astype always returns a newly allocated array. If this is set to false, the input internal array is replaced instead of returning a new NDArray with the casted data.</param>
        /// <returns>An <see cref="NDArray"/> of given <paramref name="dtype"/>.</returns>
        /// <remarks>https://docs.scipy.org/doc/numpy/reference/generated/numpy.ndarray.astype.html</remarks>
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public NDArray astype(Type dtype, bool copy = true)
        {
            return TensorEngine.Cast(this, dtype, copy);
        }

        /// <summary>
        ///     Copy of the array, cast to a specified type.
        /// </summary>
        /// <param name="dtype">The dtype to cast this array.</param>
        /// <param name="copy">By default, astype always returns a newly allocated array. If this is set to false, the input internal array is replaced instead of returning a new NDArray with the casted data.</param>
        /// <returns>An <see cref="NDArray"/> of given <paramref name="dtype"/>.</returns>
        /// <remarks>https://docs.scipy.org/doc/numpy/reference/generated/numpy.ndarray.astype.html</remarks>
        public NDArray astype(NPTypeCode typeCode, bool copy = true)
        {
            return TensorEngine.Cast(this, typeCode, copy);
        }

        /// <summary>
        /// Clone the whole NDArray
        /// internal storage is also cloned into 2nd memory area
        /// </summary>
        /// <returns>Cloned NDArray</returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Clone the whole NDArray
        /// internal storage is also cloned into 2nd memory area
        /// </summary>
        /// <returns>Cloned NDArray</returns>
        public NDArray Clone()
        {
            return new NDArray(this.Storage.Clone(), Shape.Clone(true)) {tensorEngine = TensorEngine};
        }

        public IEnumerator GetEnumerator()
        {
            if (Shape.IsSliced)
                return _sliced().GetEnumerator();

            return Array?.GetEnumerator() ?? _empty().GetEnumerator();

            IEnumerable _sliced()
            {


#if _REGEN
		    #region Compute
		    switch (GetTypeCode)
		    {
			    %foreach supported_currently_supported,supported_currently_supported_lowercase%
			    case NPTypeCode.#1:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return Get#1(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    %
			    default:
				    throw new NotSupportedException();
		    }
		    #endregion
#else
		    #region Compute
		    switch (GetTypeCode)
		    {
			    case NPTypeCode.Boolean:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetBoolean(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.Byte:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetByte(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.Int16:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetInt16(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.UInt16:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetUInt16(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.Int32:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetInt32(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.UInt32:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetUInt32(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.Int64:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetInt64(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.UInt64:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetUInt64(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.Char:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetChar(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.Double:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetDouble(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.Single:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetSingle(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    case NPTypeCode.Decimal:
			    {
				    if (size == 0)
                        yield break;

                    var incr = new NDIndexArrayIncrementor(shape);
                    do
                    {
                        yield return GetDecimal(incr.Index);
                    } while (incr.Next() != null);
                    yield break;
			    }
			    default:
				    throw new NotSupportedException();
		    }
		    #endregion
#endif

            }

            IEnumerable _empty()
            {
                yield break;
            }
        }

        /// <summary>
        /// New view of array with the same data.
        /// </summary>
        /// <returns></returns>
        public NDArray view(Type dtype = null)
        {
            if (dtype != null && dtype != this.dtype)
                Storage.ReplaceData(Array, dtype);

            var nd = new NDArray(Array, shape);

            return nd;
        }

        #region Getters

        /// <summary>
        ///     Retrieves value of type <see cref="bool"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="bool"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBoolean(params int[] indices)
        {
            return Storage.GetBoolean(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="byte"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="byte"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetByte(params int[] indices)
        {
            return Storage.GetByte(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="char"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="char"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char GetChar(params int[] indices)
        {
            return Storage.GetChar(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="decimal"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="decimal"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal GetDecimal(params int[] indices)
        {
            return Storage.GetDecimal(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="double"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="double"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetDouble(params int[] indices)
        {
            return Storage.GetDouble(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="short"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="short"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetInt16(params int[] indices)
        {
            return Storage.GetInt16(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="int"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="int"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt32(params int[] indices)
        {
            return Storage.GetInt32(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="long"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="long"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetInt64(params int[] indices)
        {
            return Storage.GetInt64(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="float"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="float"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSingle(params int[] indices)
        {
            return Storage.GetSingle(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="ushort"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="ushort"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetUInt16(params int[] indices)
        {
            return Storage.GetUInt16(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="uint"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="uint"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetUInt32(params int[] indices)
        {
            return Storage.GetUInt32(indices);
        }

        /// <summary>
        ///     Retrieves value of type <see cref="ulong"/>.
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="ulong"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetUInt64(params int[] indices)
        {
            return Storage.GetUInt64(indices);
        }

        /// <summary>
        ///     Retrieves value of unspecified type (will figure using <see cref="DType"/>).
        /// </summary>
        /// <param name="indices">The shape's indices to get.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">When <see cref="DType"/> is not <see cref="object"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetValue(params int[] indices)
        {
            return Storage.GetValue(indices);
        }

        /// <summary>
        ///     Retrieves value of 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetAtIndex(int index)
        {
            return Storage.GetAtIndex(index);
        }

        /// <summary>
        ///     Retrieves value of 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetAtIndex<T>(int index) where T : unmanaged
        {
            unsafe
            {
                return *((T*)Address + index);
            }
        }

        /// <summary>
        ///     Retrieves value of 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAtIndex(object obj, int index)
        {
            Storage.SetAtIndex(obj, index);
        }

        /// <summary>
        ///     Retrieves value of 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAtIndex<T>(T value, int index) where T : unmanaged
        {
            //TODO! it might be not wise to provide a method that can corrupt memory. should we lower performance but perform checks?
            unsafe
            {
                *((T*)Address + index) = value;
            }
        }

        //TODO! add SetInt32 and such methods!

        #endregion

        private class NDArrayDebuggerProxy
        {
            private readonly NDArray NDArray;

            /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
            public NDArrayDebuggerProxy(NDArray ndArray)
            {
                NDArray = ndArray;
            }

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString()
            {
                //TODO! make a truncuted to string.
                return NDArray.ToString(false);
            }
        }
    }
}
