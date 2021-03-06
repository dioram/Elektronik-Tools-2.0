message(STATUS "${CMAKE_MODULE_PATH}")
include(install_3rdparty)

if(WIN32)
    set(C_COMPILER "cl")
    set(CXX_COMPILER "cl")
else()
    set(C_COMPILER "${CMAKE_C_COMPILER}")
    set(CXX_COMPILER "${CMAKE_CXX_COMPILER}")
endif()

find_program(CMAKE_ASM_NASM_COMPILER nasm)

set(ARGS 
    "-DCMAKE_ASM_NASM_COMPILER=${CMAKE_ASM_NASM_COMPILER}"
    "-DCMAKE_C_COMPILER=${C_COMPILER}"
    "-DCMAKE_CXX_COMPILER=${CXX_COMPILER}"
    "-DgRPC_INSTALL_CSHARP_EXT=OFF"
    )

install_3rdparty_cmake_project(https://github.com/grpc/grpc v1.30.1 grpc ARGS)

set(CMAKE_PROGRAM_PATH "${grpc_INSTALL_DIR}/bin;${CMAKE_PROGRAM_PATH}")
set(CMAKE_PREFIX_PATH "${grpc_INSTALL_DIR};${grpc_INSTALL_DIR}/cmake;${CMAKE_PREFIX_PATH}")