using System;
using System.Reflection;
using System.Reflection.Emit;
using Celeste;

namespace CelesteDeathTracker
{
    internal static class AltSidesHelperInterop
    {
        public delegate object GetModeMetaForAltSideDelegate(AreaData data);
        public static GetModeMetaForAltSideDelegate? GetModeMetaForAltSide;

        public delegate string GetDeathsIconDelegate(object instance);
        public static GetDeathsIconDelegate? GetDeathsIcon;

        public static void CreateDelegates(Assembly assembly)
        {
            GetModeMetaForAltSide = assembly.GetType("AltSidesHelper.AltSidesHelperModule")
                ?.GetMethod("GetModeMetaForAltSide")
                ?.CreateDelegate<GetModeMetaForAltSideDelegate>();

            GetDeathsIcon = CreateAnonymousGetterDelegate<GetDeathsIconDelegate, string>(assembly, "AltSidesHelper.AltSidesHelperMode", "DeathsIcon");
        }

        public static string? GetOverrideSkullIcon(AreaData data)
        {
            var meta = GetModeMetaForAltSide?.Invoke(data);
            return meta == null ? null : GetDeathsIcon?.Invoke(meta);
        }

        private static TDelegate? CreateAnonymousGetterDelegate<TDelegate, TReturn>(Assembly assembly, string typeName, string propertyName) where TDelegate : Delegate
        {
            var type = assembly.GetType(typeName);
            if (type == null) return null;

            var property = type.GetProperty(propertyName);
            if (property == null || property.GetMethod == null) return null;

            var dynamicMethod = new DynamicMethod("get_" + propertyName + "_generated", typeof(TReturn), [typeof(object)]);
            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, type);
            il.EmitCall(OpCodes.Callvirt, property.GetMethod, null);
            il.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate<TDelegate>();
        }
    }
}
