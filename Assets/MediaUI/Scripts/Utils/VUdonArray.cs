/*
This script is a more compact version of Varneon's Array Extensions wrapped
into a namespace to avoid any conflicts and simplify import process

Used only methods utilized in the search script and UI

Original link - https://github.com/Varneon/VUdon-ArrayExtensions
Varneon's GitHub - https://github.com/Varneon



Copy of the original license:

MIT License

Copyright (c) 2022 Varneon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


using System;
using UnityEngine;


namespace Arti
{
    public static class ArrayUtils
    {
        #region System.Collections.Generic.List<T>

        // <summary>
        /// Adds an object to the end of the array
        /// <para>
        /// Based on: <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.add?view=net-6.0">List&lt;T&gt;.Add(T)</see>
        /// </para>
        /// </summary>
        /// <returns>Modified T[]</returns>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">Source T[] to modify.</param>
        /// <param name="item">The object to be added to the end of the T[].</param>
        public static T[] Add<T>(this T[] array, T item)
        {
            int length = array.Length;
            T[] newArray = new T[length + 1];
            array.CopyTo(newArray, 0);
            newArray.SetValue(item, length);
            return newArray;
        }


        /// <summary>
        /// Resizes the array
        /// </summary>
        /// <returns>Modified T[]</returns>
        /// <typeparam name="T">The type of elements in the array.</typeparam>
        /// <param name="array">Source T[] to modify.</param>
        /// <param name="newSize">New size of the array.</param>
        public static T[] Resize<T>(this T[] array, int newSize)
        {
            if (newSize < 0) { newSize = 0; }
            T[] newArray = new T[newSize];
            Array.Copy(array, 0, newArray, 0, Mathf.Min(newSize, array.Length));
            return newArray;
        }


        #endregion
    }
}