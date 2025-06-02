using FlatOut2.SDK;
using FlatOut2.SDK.API;
using FlatOut2_ShaderSwapper.Template;
using Microsoft.VisualBasic;
//using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X86;
using static Reloaded.Hooks.Definitions.X86.FunctionAttribute;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Runtime.InteropServices;

using unsafe D3DXHANDLE = byte*;

namespace FlatOut2_ShaderSwapper
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        [StructLayout(LayoutKind.Explicit)]
        private unsafe struct Shader
        {
            [FieldOffset(0x0)]
            public void* VFTable;

            [FieldOffset(0x8)]
            public int Param2;

            [FieldOffset(0x50)]
            public uint Flags;

            [FieldOffset(0x54)]
            public void* Effect;

            [FieldOffset(0x68)]
            public D3DXHANDLE Tex0;

            [FieldOffset(0x6C)]
            public D3DXHANDLE Tex1;

            [FieldOffset(0x70)]
            public D3DXHANDLE Tex2;

            [FieldOffset(0x74)]
            public D3DXHANDLE Tex3;

            [FieldOffset(0x78)]
            public D3DXHANDLE MCub;

            [FieldOffset(0x7C)]
            public D3DXHANDLE DFac;

            [FieldOffset(0x80)]
            public D3DXHANDLE VDiff;
        }

        [DllImport("dxStuff.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static unsafe extern void RecompileShader(void* shaderPtr, char* shader, uint shaderLen);

        private unsafe struct FO2Shader(Shader* shader, string filename)
        {
            public Shader* Shader = shader;
            public string Filename = filename;
            public DateTime LastModified;
        }

        private static readonly FO2Shader[] Shaders = new FO2Shader[50];
        private static int ShaderCount;

        [Function([Register.eax, Register.esi], Register.eax, StackCleanup.Callee)]
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate void* Shader_ShaderPtr(void* effect_EAX, Shader* shader_ESI, string filename, int param_2);


        private static Reloaded.Hooks.Definitions.IHook<Shader_ShaderPtr> ShaderShaderHook;

        private unsafe void* NewShader_Shader(void* effect_EAX, Shader* shader_ESI, string filename, int param_2)
        {
            if (File.Exists(filename))
                Shaders[ShaderCount++] = new(shader_ESI, filename);

            return ShaderShaderHook.OriginalFunction(effect_EAX, shader_ESI, filename, param_2);
        }

        private static unsafe void PerFrame()
        {
            for (int i = 0; i < ShaderCount; i++)
            {
                DateTime dateModified;
                try
                {
                    dateModified = FileSystem.FileDateTime(Shaders[i].Filename);
                }
                catch (Exception e)
                {
                    return;
                }

                if (dateModified > Shaders[i].LastModified)
                {
                    string shaderText = "";
                    // Race conditions
                    try
                    {
                        shaderText = File.ReadAllText(Shaders[i].Filename);
                    }
                    catch (Exception e)
                    {
                        return;
                    }

                    RecompileShader(Shaders[i].Shader, (char*)Marshal.StringToHGlobalAnsi(shaderText), (uint)shaderText.Length);
                    Shaders[i].LastModified = dateModified;
                }
            }
        }



        public unsafe Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _modConfig = context.ModConfig;


            // For more information about this template, please see
            // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

            // If you want to implement e.g. unload support in your mod,
            // and some other neat features, override the methods in ModBase.

            // TODO: Implement some mod logic

            SDK.Init(_hooks!);
            Helpers.HookPerFrame(PerFrame);

            ShaderShaderHook = _hooks!.CreateHook<Shader_ShaderPtr>(NewShader_Shader, 0x005ACBD0).Activate();
        }


        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}