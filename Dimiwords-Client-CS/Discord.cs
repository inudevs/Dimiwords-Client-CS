using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static Dimiwords_Client_CS.DiscordRpc;

namespace Dimiwords_Client_CS
{
    class Discord
    {
        static long time = DateTime.Now.Ticks;
        static RichPresence presence;
        static EventHandlers handlers;

        //dll 이름
        public const string DLL = "discord-rpc.dll";
        /// <summary>
        /// 디스코드 기능을 사용하기 위한 dll이 존재하는지 체크하는 함수(메서드)
        /// </summary>
        /// <returns>
        /// true일 경우 dll이 존재
        /// false일 경우 dll이 존재하지 않음
        /// </returns>
        public static bool LibCheck()
        {
            return File.Exists(DLL);
        }
        
        private static void Initialize(string clientId)
        {
            handlers = new EventHandlers
            {
                readyCallback = ReadyCallback
            };
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;

            DiscordRpc.Initialize(clientId, ref handlers, true, null);
        }

        //한글 깨져서 고치려고 만듬
        private static IntPtr C2Ptr(string str)
        {
            //넘겨받은 인자가 null이 아닐때
            if (str != null)
            {
                //byte[]로 변경
                var retArray = Encoding.UTF8.GetBytes(str);
                //IntPtr로 변경
                var retPtr = Marshal.AllocHGlobal(retArray.Length + 1);
                //복사
                Marshal.Copy(retArray, 0, retPtr, retArray.Length);
                //새로 써버리기
                Marshal.WriteByte(retPtr, retArray.Length, 0);
                //반환
                return retPtr;
            }
            //null이므로 IntPtr.Zero 반환
            return IntPtr.Zero;
        }

        //디스코드 상태 업데이트
        public static void StateUpdate(string state)
        {
            UpdatePresence(state);
        }

        //디스코드 업데이트
        private static void UpdatePresence(string state)
        {
            //디테일
            presence.details = C2Ptr("https://dimiwords.tk");
            //상태
            presence.state = C2Ptr(state);
            //시작 시간
            presence.startTimestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            //끝난 시간
            //presence.endTimestamp = (DateTime.Now.Ticks + 5) * 60;
            //사진
            presence.largeImageKey = "logo";
            presence.largeImageText = null;
            presence.smallImageKey = null;
            presence.smallImageText = null;

            //디스코드 dll 업데이트 메서드
            DiscordRpc.UpdatePresence(ref presence);
        }

        //디스코드에 연결
        public static void Start()
        {
            var clientId = "528625130012934174";
            Initialize(clientId);
        }

        //연결 끊기
        public static void Shutdown()
        {
            DiscordRpc.Shutdown();
        }

        #region 디버깅 전용
        //준비됨을 알림
        private static void ReadyCallback()
        {
            Debug.WriteLine("Ready");
        }

        //연결이 끊김을 알림
        public static void DisconnectedCallback(int errorCode, string message)
        {
            Debug.WriteLine($"Disconnect {errorCode}: {message}");
        }

        //에러내용을 알림
        private static void ErrorCallback(int errorCode, string message)
        {
            Debug.WriteLine($"Error {errorCode}: {message}");
        }
        #endregion
    }

    class DiscordRpc
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ReadyCallback();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DisconnectedCallback(int errorCode, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ErrorCallback(int errorCode, string message);

        public struct EventHandlers
        {
            public ReadyCallback readyCallback;
            public DisconnectedCallback disconnectedCallback;
            public ErrorCallback errorCallback;
        }
        
        [Serializable]
        public struct RichPresence
        {
            public IntPtr state; //최대 128 bytes
            public IntPtr details; //최대 128 bytes
            public long startTimestamp;
            public long endTimestamp;
            public string largeImageKey; //최대 32 bytes
            public string largeImageText; //최대 128 bytes
            public string smallImageKey; //최대 32 bytes
            public string smallImageText; //최대 128 bytes
            public string partyId; //최대 128 bytes
            public int partySize;
            public int partyMax;
            public string matchSecret; //최대 128 bytes
            public string joinSecret; //최대 128 bytes
            public string spectateSecret; //최대 128 bytes
            public bool instance;
        }

        #region DLL
        [DllImport(Discord.DLL, EntryPoint = "Discord_Initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Initialize(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId);

        [DllImport(Discord.DLL, EntryPoint = "Discord_UpdatePresence", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdatePresence(ref RichPresence presence);

        [DllImport(Discord.DLL, EntryPoint = "Discord_Shutdown", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();
        #endregion
    }
}
