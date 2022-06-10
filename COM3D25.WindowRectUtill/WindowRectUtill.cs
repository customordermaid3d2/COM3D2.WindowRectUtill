using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LillyUtill.MyWindowRect
{

    public class WindowRectUtill
    {
        internal ManualLogSource log;

        //private float windowSpace;
        private Rect windowRect;
        private Size windowRectOpen;
        private Size windowRectClose;
        //private Position position;
        //private string jsonPath;

        //private ConfigFile config;

        private ConfigEntry<bool> isOpen;
        private ConfigEntry<bool> isGUIOn;
        private ConfigEntry<bool> isLog;
        private ConfigEntry<bool> isSkip;

        private ConfigEntry<float> x;
        private ConfigEntry<float> y;

        private ConfigEntry<float> sL;
        private ConfigEntry<float> sR;
        private ConfigEntry<float> sT;
        private ConfigEntry<float> sB;

        //internal event Action<int> actionWindowFunctionBody = (x) => { };
        public delegate void delegateWindowFunctionBody(int id);
        delegateWindowFunctionBody windowFunctionBody;// = WindowFunctionBody;

        private static ConfigEntry<int> vGUISortX;
        private static ConfigEntry<int> vGUISortY;
        private static ConfigEntry<int> vGUISortDW;
        private static ConfigEntry<int> vGUISortDH;


        internal static event Action actionSave;


        internal static readonly List<WindowRectUtill> myWindowRects = new List<WindowRectUtill>();

        internal static void ActionSave()
        {
            actionSave();
        }

        public bool IsSkip
        {
            get => isSkip.Value;
            set => isSkip.Value = value;
        }

        public bool IsGUIOn
        {
            get => isGUIOn.Value;
            set => isGUIOn.Value = value;
        }

        public bool IsOpen
        {
            get => isOpen.Value;
            set
            {
                if (isOpen.Value != value)
                {
                    if (value)
                    {
                        windowRect.width = windowRectOpen.w;
                        windowRect.height = windowRectOpen.h;
                        windowRect.x -= windowRectOpen.w - windowRectClose.w;
                        windowName = FullName;
                    }
                    else
                    {
                        windowRect.width = windowRectClose.w;
                        windowRect.height = windowRectClose.h;
                        windowRect.x += windowRectOpen.w - windowRectClose.w;
                        windowName = ShortName;
                    }
                    isOpen.Value = value;
                }
            }
        }

        public string windowName;
        public string FullName;
        public string ShortName;

        private static event Action<int, int> actionScreen;

        internal static int widthbak;
        internal static int heightbak;

        struct Position
        {
            public float x;
            public float y;

            public Position(float x, float y) : this()
            {
                this.x = x;
                this.y = y;
            }
        }

        public struct Size
        {
            public float w;
            public float h;

            public Size(float w, float h) : this()
            {
                this.w = w;
                this.h = h;
            }
        }

        public Rect WindowRect
        {
            get
            {
                return windowRect;
            }
            set
            {
                // 윈도우 리사이즈시 밖으로 나가버리는거 방지
                windowRect = value;
                windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + sL.Value, Screen.width - sR.Value);
                windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + sT.Value, Screen.height - sB.Value);
            }
        }

        public float Height { get => windowRect.height; set => windowRect.height = value; }

        public float Width { get => windowRect.width; set => windowRect.width = value; }

        public float X
        {
            get => windowRect.x;
            set
            {
                windowRect.x = value;
                windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + sL.Value, Screen.width - sR.Value);
            }
        }

        public float Y
        {
            get => windowRect.y;
            set
            {
                windowRect.y = value;
                windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + sT.Value, Screen.height - sB.Value);
            }
        }

        public Size WindowRectOpen { get => windowRectOpen; }

        public Size WindowRectClose { get => windowRectClose; }

        public int winNum;
        public static int winCnt;
        
        public static WindowRectUtill Create(ConfigFile config, ManualLogSource logger, string fileName, string windowFullName, string windowShortName, float wc = 100f, float wo = 300f, float hc = 30f, float ho = 600f, float x = 30f, float y = 30f, float sL = 30f, float sR = 30f, float sT = 30f, float sB = 30f)
        {
            return new WindowRectUtill(config, logger,  windowFullName, windowShortName, windowFunctionBody: null, wc, wo, hc, ho, x, y, sL, sR, sT, sB);
        }

        public static WindowRectUtill Create(ConfigFile config, ManualLogSource logger, string windowFullName, string windowShortName, delegateWindowFunctionBody windowFunctionBody = null, float wc = 100f, float wo = 300f, float hc = 30f, float ho = 600f, float x = 30f, float y = 30f, float sL = 30f, float sR = 30f, float sT = 30f, float sB = 30f)
        {
            return new WindowRectUtill(config, logger,  windowFullName, windowShortName, windowFunctionBody, wc, wo, hc, ho, x, y, sL, sR, sT, sB);
        }
        
        public static WindowRectUtill Create(delegateWindowFunctionBody windowFunctionBody ,ConfigFile config, ManualLogSource logger, string windowFullName, string windowShortName)
        {
            return new WindowRectUtill(config, logger,  windowFullName, windowShortName, windowFunctionBody);
        }

        public WindowRectUtill(ConfigFile config, ManualLogSource logger, string fileName, string windowFullName, string windowShortName, float wc = 100f, float wo = 300f, float hc = 30f, float ho = 600f, float x = 30f, float y = 30f, float sL = 30f, float sR = 30f, float sT = 30f, float sB = 30f)                        
        {
            CreateMaster(config, windowFullName, windowShortName, logger, WindowFunctionBody, wc, wo, hc, ho, x, y, sL, sR, sT, sB);
        }

        public WindowRectUtill(ConfigFile config, ManualLogSource logger, string fileName, string windowFullName, string windowShortName, float wc = 100f, float wo = 300f, float hc = 30f, float ho = 600f, float x = 30f, float y = 30f, float windowSpace = 30f)                         
        {
            CreateMaster(config,windowFullName, windowShortName, logger, WindowFunctionBody, wc, wo, hc, ho, x, y, windowSpace, windowSpace, windowSpace, windowSpace);
        }

        ///
        public WindowRectUtill(ConfigFile config, string fileName, string windowFullName, string windowShortName, float wc = 100f, float wo = 300f, float hc = 30f, float ho = 600f, float x = 30f, float y = 30f, float windowSpace = 30f)             
        {
            CreateMaster(config,  windowFullName, windowShortName, WindowRectGUI.log, WindowFunctionBody, wc, wo, hc, ho, x, y, windowSpace, windowSpace, windowSpace, windowSpace);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="fileName">not use</param>
        /// <param name="windowFullName"></param>
        /// <param name="windowShortName"></param>
        /// <param name="windowFunctionBody"></param>
        /// <param name="wc"></param>
        /// <param name="wo"></param>
        /// <param name="hc"></param>
        /// <param name="ho"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="sL"></param>
        /// <param name="sR"></param>
        /// <param name="sT"></param>
        /// <param name="sB"></param>
        public WindowRectUtill(ConfigFile config, ManualLogSource logger, string fileName, string windowFullName, string windowShortName, delegateWindowFunctionBody windowFunctionBody = null, float wc = 100f, float wo = 300f, float hc = 30f, float ho = 600f, float x = 30f, float y = 30f, float sL = 30f, float sR = 30f, float sT = 30f, float sB = 30f)               
        {
            CreateMaster(config, windowFullName, windowShortName, logger, windowFunctionBody, wc, wo, hc, ho, x, y, sL, sR, sT, sB);
        }
        
        public WindowRectUtill(ConfigFile config, ManualLogSource logger,  string windowFullName, string windowShortName, delegateWindowFunctionBody windowFunctionBody = null, float wc = 100f, float wo = 300f, float hc = 30f, float ho = 600f, float x = 30f, float y = 30f, float sL = 30f, float sR = 30f, float sT = 30f, float sB = 30f)
        {
            CreateMaster(config, windowFullName, windowShortName, logger, windowFunctionBody, wc, wo, hc, ho, x, y, sL, sR, sT, sB);
        }     

        public WindowRectUtill(delegateWindowFunctionBody windowFunctionBody , ConfigFile config, ManualLogSource logger,  string windowFullName, string windowShortName, float wc = 100f, float wo = 300f, float hc = 30f, float ho = 600f, float x = 30f, float y = 30f, float sL = 30f, float sR = 30f, float sT = 30f, float sB = 30f)
        {
            CreateMaster(config, windowFullName, windowShortName, logger, windowFunctionBody, wc, wo, hc, ho, x, y, sL, sR, sT, sB);
        }

        public WindowRectUtill(delegateWindowFunctionBody windowFunctionBody , ConfigFile config, ManualLogSource logger,  string windowFullName, string windowShortName)
        {
            CreateMaster(config, windowFullName, windowShortName, logger, windowFunctionBody);
        }

        private void CreateMaster(ConfigFile config, string windowFullName, string windowShortName, ManualLogSource logger=null, delegateWindowFunctionBody windowFunctionBody = null, float wc = 100f, float wo = 300f, float hc = 30f, float ho = 600f, float x = 30f, float y = 30f, float sL = 30f, float sR = 30f, float sT = 30f, float sB = 30f)
        {           
            if (logger != null)
            {
                log = logger;
            }
            else
            {
                log = WindowRectGUI.log;
            }            
            if (windowFunctionBody != null)
            {
                this.windowFunctionBody = windowFunctionBody;
            }
            else
            {
                this.windowFunctionBody = WindowFunctionBody;
            }

            winNum = 7777 + winCnt++;
            myWindowRects.Add(this);

            isOpen = config.Bind("GUI", "isOpen", true);
            isGUIOn = config.Bind("GUI", "isGUIOn", false);
            isLog = config.Bind("GUI", "isLog", false);
            isSkip = config.Bind("GUI", "isSkip", false);

            this.x = config.Bind("GUI", "x", x);
            this.y = config.Bind("GUI", "y", y);

            this.sL = config.Bind("GUI", "space L", sL);
            this.sR = config.Bind("GUI", "space R", sR);
            this.sT = config.Bind("GUI", "space T", sT);
            this.sB = config.Bind("GUI", "space B", sB);

            windowName = windowFullName;
            FullName = windowFullName;
            ShortName = windowShortName;

            //this.windowSpace = windowSpace;

            windowRect = new Rect(x, y, wo, ho);
            windowRectOpen = new Size(wo, ho);
            windowRectClose = new Size(wc, hc);

            //jsonPath = Path.GetDirectoryName(config.ConfigFilePath) + $@"\{fileName}-rect.json";

            IsOpen = !(IsOpen = !isOpen.Value);

            actionSave += save;
            actionScreen += ScreenChg;

            load();
        }



        internal static void init(ConfigFile config)
        {
            if (vGUISortX == null) vGUISortX = config.Bind("GUI", "vGUISortX", 0);
            if (vGUISortY == null) vGUISortY = config.Bind("GUI", "vGUISortY", 70);
            if (vGUISortDW == null) vGUISortDW = config.Bind("GUI", "vGUISortDW", 0);
            if (vGUISortDH == null) vGUISortDH = config.Bind("GUI", "vGUISortDH", 30);
        }

        private void load()
        {
            windowRect.x = x.Value;
            windowRect.y = y.Value;
        }

        private void save()
        {
            LogDebug($"save {windowRect}");
            x.Value = windowRect.x;
            y.Value = windowRect.y;
        }

        private void LogDebug(string v)
        {
            if (isLog.Value)
            {
                if (log == null)
                {
                    log.LogDebug(v);
                }
                else
                {
                    UnityEngine.Debug.Log(v);
                }
            }
        }

        /// <summary>
        /// 생각보다 의도대로 안됨. 특히 최대화시
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void ScreenChg(int width, int height)
        {
            if ((windowRect.x + windowRect.width) > widthbak / 2)
            {
                X += width - widthbak;
            }
            if ((windowRect.y + windowRect.height) > heightbak / 2)
            {
                Y += height - heightbak;
            }
            //MyLog.LogMessage("SetResolution3", widthbak, heightbak, Screen.fullScreen);
            //MyLog.LogMessage("SetResolution4", Screen.width, Screen.height, Screen.fullScreen);
            //MyLog.LogMessage("SetResolution5", windowRect.x, windowRect.y);
        }


        public bool IsGUIOnOffChg()
        {
            return IsGUIOn = !IsGUIOn;
        }
        
        public bool IsGUIOpenCloseChg()
        {
            return IsOpen = !IsOpen;
        }

        public void GUILayoutTop()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(windowName, GUILayout.Height(20));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { IsOpen = !IsOpen; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { IsGUIOn = false; }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// WindowRect = GUILayout.Window(winNum, WindowRect, WindowFunction, "", GUI.skin.box);
        /// </summary>
        public void OnGUI()
        {
            if (!IsGUIOn)
                return;

            WindowRect = GUILayout.Window(winNum, WindowRect, WindowFunction, "", GUI.skin.box);
        }

        private Vector2 scrollPosition;

        /// <summary>
        /// actionWindowFunctionBody(id);
        /// </summary>
        /// <param name="id"></param>
        public void WindowFunction(int id)
        {
            GUI.enabled = true;

            GUILayoutTop();

            if (!IsOpen)
            {
            }
            else
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                //actionWindowFunctionBody(id);
                windowFunctionBody(id);

                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
        }

        public virtual void WindowFunctionBody(int id)
        {

        }

        [HarmonyPatch(typeof(Screen), "SetResolution", typeof(int), typeof(int), typeof(bool))]
        [HarmonyPostfix]
        private static void SetResolutionPost(int width, int height, bool fullscreen)
        {
            actionScreen(width, height);
            widthbak = width;
            heightbak = height;
            //MyLog.LogMessage("SetResolution");
            WindowRectGUI.log?.LogInfo($"{width} , {height} , {fullscreen}");
        }

        /// <summary>
        /// 우측 정렬
        /// </summary>
        public static void GUISortIsOpen()
        {
            int i = 0, x = vGUISortX.Value, y = vGUISortY.Value, w = vGUISortDW.Value, h = vGUISortDH.Value;
            foreach (var item in myWindowRects)
            {
                if (item.IsSkip) continue;
                if (item.IsOpen)
                {
                    item.X = Screen.width - x - item.Width - w * i;
                    item.Y = y + h * i++;
                }
            }
        }

        /// <summary>
        /// 우측 정렬
        /// </summary>
        public static void GUISortIsGUIOn()
        {
            int i = 0, x = vGUISortX.Value, y = vGUISortY.Value, w = vGUISortDW.Value, h = vGUISortDH.Value;
            foreach (var item in myWindowRects)
            {
                if (item.IsSkip) continue;
                if (item.IsGUIOn)
                {
                    item.X = Screen.width - x - item.Width - w * i;
                    item.Y = y + h * i++;
                }
            }
        }

        /// <summary>
        /// 우측 정렬
        /// </summary>
        public static void GUISortAll()
        {
            int i = 0, x = vGUISortX.Value, y = vGUISortY.Value, w = vGUISortDW.Value, h = vGUISortDH.Value;
            foreach (var item in myWindowRects)
            {
                if (item.IsSkip) continue;
                item.X = Screen.width - x - item.Width - w * i;
                item.Y = y + h * i++;
            }
        }

        public static void GUIOpenAll()
        {
            foreach (var item in myWindowRects)
            {
                if (item.IsSkip) continue;
                item.IsOpen = true;
            }
        }
        
        public static void GUICloseAll()
        {
            foreach (var item in myWindowRects)
            {
                if (item.IsSkip) continue;
                item.IsOpen = false;
            }
        }

        public static void GUIOffAll()
        {
            foreach (var item in myWindowRects)
            {
                if (item.IsSkip) continue;
                item.IsGUIOn = false;
            }
        }
        
        public static void GUIOnAll()
        {
            foreach (var item in myWindowRects)
            {
                if (item.IsSkip) continue;                              
                item.IsGUIOn = true;
            }
        }



    }
}
