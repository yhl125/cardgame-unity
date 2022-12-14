using System;
using UnityEngine;

public static class PlatformConfig
{
    public const string APIUri = "https://j7s0ihgvz4.execute-api.ap-northeast-2.amazonaws.com";

    //     void Awake()
//     {
// #if UNITY_EDITOR
//         Environment.SetEnvironmentVariable("API_URI", "https://j7s0ihgvz4.execute-api.ap-northeast-2.amazonaws.com");
//
// #elif UNITY_IOS
//         Environment.SetEnvironmentVariable("API_URI", "http://localhost:8000");
//
// #elif UNITY_ANDROID
//         Environment.SetEnvironmentVariable("API_URI", "http://localhost:8000");
//         
// #elif UNITY_STANDALONE_OSX
//         Environment.SetEnvironmentVariable("API_URI", "https://j7s0ihgvz4.execute-api.ap-northeast-2.amazonaws.com");
//         
// #elif UNITY_STANDALONE_WIN
//         Environment.SetEnvironmentVariable("API_URI", "https://j7s0ihgvz4.execute-api.ap-northeast-2.amazonaws.com");
// #else
//         Environment.SetEnvironmentVariable("API_URI", "https://j7s0ihgvz4.execute-api.ap-northeast-2.amazonaws.com");
// #endif
//     }
}