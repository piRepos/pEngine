using System;
using System.Runtime.InteropServices;

namespace pEngine.Graphics.Shading
{
    /// <summary>
    /// Mini wrapper for the shaderc Google library.
    /// You can find more information in the shaderc repository:
    /// https://github.com/google/shaderc
    /// </summary>
    public class ShaderCompiler : IDisposable
    {
        /// <summary>
        /// Makes a new instance of <see cref="ShaderCompiler"/> class.
        /// </summary>
        public ShaderCompiler()
        {
            // - Create a shaderc compiler instance
            pHandle = shaderc_compiler_initialize();
        }

        /// <summary>
        /// Internal C++ object handle.
        /// </summary>
        private IntPtr pHandle { get; set; }

        /// <summary>
        /// Dispose logic variable.
        /// </summary>
        private bool pDisposed { get; set; }

        /// <summary>
        /// Implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals <see cref="true"/>, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals <see cref="false"/>, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"><see cref="True"/> if called from user's code.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!pDisposed)
            {
                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                shaderc_compiler_release(pHandle);
                pHandle = IntPtr.Zero;

                // Note disposing has been done.
                pDisposed = true;

            }
        }

        /// <summary>
        /// Compiles the specified source GLSL450 code to a Vulkan SPIR-V binary.
        /// </summary>
        /// <param name="source">GLSL450 source code.</param>
        /// <param name="kind">Shader type.</param>
        /// <param name="entry">Main entry point function name.</param>
        /// <returns>An instance of <see cref="CompileResult"/> struct containing the compilation result.</returns>
        public CompileResult CompileToSpirV(string source, ShaderKind kind, string entry)
        {
            // - Executes extern library compilation and takes the result as a pointer
            IntPtr result = shaderc_compile_into_spv(pHandle, source, source.Length, kind, "", entry, IntPtr.Zero);

            // - Gets the compilation status and metadata
            CompilationStatus status = shaderc_result_get_compilation_status(result);
            uint errors = shaderc_result_get_num_errors(result);
            uint warnings = shaderc_result_get_num_warnings(result);

            if (status != CompilationStatus.Success)
            {
                switch (status)
                {
                    case CompilationStatus.InternalError:
                    case CompilationStatus.CompilationError:
                        return new CompileResult
                        {
                            WarningCount = warnings,
                            ErrorCount = errors,
                            Errors = shaderc_result_get_error_message(result),
                            Status = status,
                            Binary = new uint[] { }
                        };
                    default:
                        throw new Exception("Unknow error");
                }
            }

            // - Gets the result's binary size
            uint binarySize = shaderc_result_get_length(result);

            // - Prepare output binary
            byte[] binary = new byte[binarySize];
            uint[] outCode = new uint[binarySize / 4];

            // - Gets shader data from the compiler
            IntPtr bytes = shaderc_result_get_bytes(result);

            // - Copy shader binary to a managed array
            Marshal.Copy(bytes, binary, 0, (int)binarySize);
            Buffer.BlockCopy(binary, 0, outCode, 0, (int)binarySize);

            // - Release result resources
            shaderc_result_release(result);

            return new CompileResult
            {
                WarningCount = warnings,
                ErrorCount = errors,
                Errors = "",
                Status = status,
                Binary = outCode
            };
        }

        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method does not get called.
        /// </summary>
        ~ShaderCompiler()
        {
            Dispose(false);
        }



        public enum ShaderKind
		{
			VertexShader,
			FragmentShader,
			ComputeShader,
			GeometryShader,
			TessControlShader,
			TessEvalShader
		}

        public enum CompilationStatus
        {
            Success = 0,
            InvalidStage = 1,
            CompilationError = 2,
            InternalError = 3,
            NullResultObject = 4,
            InvalidAssembly = 5,
            ValidationError = 6,
            TransformationError = 7,
            ConfigurationError = 8,
        }

        public struct CompileResult
        {
            /// <summary>
            /// Binary program.
            /// </summary>
            public uint[] Binary { get; set; }

            /// <summary>
            /// Compilation output errors.
            /// </summary>
            public string Errors { get; set; }

            /// <summary>
            /// Compilation warnings count.
            /// </summary>
            public uint WarningCount { get; set; }

            /// <summary>
            /// Compilation errors count.
            /// </summary>
            public uint ErrorCount { get; set; }

            /// <summary>
            /// Result compilation status.
            /// </summary>
            public CompilationStatus Status { get; set; }
        }

		[DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr shaderc_compiler_initialize();

        [DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void shaderc_compiler_release(IntPtr compiler);

        [DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static extern IntPtr shaderc_compile_into_spv(IntPtr compiler, [MarshalAs(UnmanagedType.LPStr)] string source, int sourceLen, ShaderKind shader_kind, [MarshalAs(UnmanagedType.LPStr)] string input_file_name, [MarshalAs(UnmanagedType.LPStr)] string entry_point_name, IntPtr additional_options);

		[DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr shaderc_result_get_bytes(IntPtr res);

		[DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern uint shaderc_result_get_length(IntPtr res);

		[DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		static extern string shaderc_result_get_error_message(IntPtr res);

        [DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern CompilationStatus shaderc_result_get_compilation_status(IntPtr res);

        [DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern void shaderc_result_release(IntPtr res);

        [DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern uint shaderc_result_get_num_errors(IntPtr res);

        [DllImport("shaderc_shared.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern uint shaderc_result_get_num_warnings(IntPtr res);

    }
}
