#!/usr/bin/env bash

set -ue

iceVersion="3.7.9"
iceRpcVersion="0.1.0-preview2"

usage()
{
    echo "Usage: build [command] [arguments]"
    echo "Commands (defaults to build):"
    echo "  build                     Build tests."
    echo "  clean                     Clean tests."
    echo "  rebuild                   Rebuild tests."
    echo "  test                      Runs tests."
    echo "Arguments:"
    echo "  --config | -c             Build configuration: debug or release, the default is debug."
    echo "  --ice-version             Build tests using the given Ice version default is (3.7.9)."
    echo "  --icerpc-version          Build tests using the given IceRPC version default is (0.1.0-preview2)."
    echo "  --help   | -h             Print help and exit."
}

build()
{
    run_command dotnet "build" "-c" "$dotnet_config" "/p:IceVersion=$iceVersion" "/p:IceRpcVersion=$iceRpcVersion"
}

clean()
{
    run_command dotnet clean
}

rebuild()
{
    clean
    build
}

run_test()
{
    arguments=("test" "--no-build" "-c" "$dotnet_config")
    run_command dotnet "${arguments[@]}"
}

run_command()
{
    echo "$@"
    "$@"
    exit_code=$?
    if [ $exit_code -ne 0 ]; then
        echo "Error $exit_code"
        exit $exit_code
    fi
}

action=""
config=""
while [[ $# -gt 0 ]]; do
    key="$1"
    case $key in
        -h|--help)
            usage
            exit 0
            ;;
        -c|--config)
            config=$2
            shift
            shift
            ;;
        --ice-version)
            iceVersion=$2
            shift
            shift
            ;;
        --icerpc-version)
            iceRpcVersion=$2
            shift
            shift
            ;;
        *)
            if [ -z "$action" ]
            then
                action=$1
            else
                echo "too many arguments " "$1"
                usage
                exit 1
            fi
            shift
            ;;
    esac
done

if [ -z "$action" ]
then
    action="build"
fi

if [ -z "$config" ]
then
    config="debug"
fi

actions=("build" "clean" "rebuild" "test")
if [[ ! " ${actions[*]} " == *" ${action} "* ]]; then
    echo "invalid action: " $action
    usage
    exit 1
fi

configs=("debug" "release")
if [[ ! " ${configs[*]} " == *" ${config} "* ]]; then
    echo "invalid config: " $config
    usage
    exit 1
fi

if [ "$config" == "release" ]; then
    dotnet_config="Release"
else
    dotnet_config="Debug"
fi

case $action in
    "build")
        build
        ;;
    "rebuild")
        rebuild
        ;;
    "clean")
        clean
        ;;
    "test")
        run_test
        ;;
    "doc")
        doc
        ;;
esac
