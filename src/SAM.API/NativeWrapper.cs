/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

namespace SAM.API;

#pragma warning disable SA1600 // Elements should be documented

public abstract class NativeWrapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TNativeFunctions> : INativeWrapper
{
    protected IntPtr ObjectAddress;
    protected TNativeFunctions? Functions;

    public override string ToString()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            "Steam Interface<{0}> #{1:X8}",
            typeof(TNativeFunctions),
            ObjectAddress.ToInt32());
    }

    public void SetupFunctions(IntPtr objectAddress)
    {
        ObjectAddress = objectAddress;

        var iface = Marshal.PtrToStructure<NativeClass>(
            ObjectAddress);

        Functions = Marshal.PtrToStructure<TNativeFunctions>(
            iface.VirtualTable);
    }

    readonly Dictionary<IntPtr, Delegate> _FunctionCache = [];

    protected TDelegate GetDelegate<TDelegate>(IntPtr pointer) where TDelegate : Delegate
    {
        TDelegate function;

        if (_FunctionCache.ContainsKey(pointer) == false)
        {
            function = Marshal.GetDelegateForFunctionPointer<TDelegate>(pointer);
            _FunctionCache[pointer] = function;
        }
        else
        {
            function = (TDelegate)_FunctionCache[pointer];
        }

        return function;
    }

    protected TDelegate GetFunction<TDelegate>(IntPtr pointer)
        where TDelegate : Delegate
    {
        return GetDelegate<TDelegate>(pointer);
    }

    protected void Call<TDelegate>(IntPtr pointer, params object[] args) where TDelegate : Delegate
    {
        try
        {
            GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
        }
        catch (Exception)
        {
            throw;
        }
    }

    protected TReturn? Call<TReturn, TDelegate>(IntPtr pointer, params object[] args) where TDelegate : Delegate
    {
        try
        {
            return (TReturn?)GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
        }
        catch (Exception)
        {
            throw;
        }
    }
}