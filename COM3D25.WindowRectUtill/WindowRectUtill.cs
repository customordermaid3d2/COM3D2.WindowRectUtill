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
        private float windowSpace;
        private Rect windowRect;
        private Size windowRectOpen;
        private Size windowRectClose;
        //private Position position;
        //private string jsonPath;
              
        //private ConfigFile config;

        private ConfigEntry<bool> isOpen;
        private ConfigEntry<bool> isGUIOn;
        private ConfigEntry<bool> isLog;

        private ConfigEntry<float> x;
        private ConfigEntry<float> y;

        private static ConfigEntry<int> vGUISortX;
        private static ConfigEntry<int> vGUISortY;
        private static ConfigEntry<int> vGUISortDW;
        private static ConfigEntry<int> vGUISortDH;

        internal static ManualLogSource log;

        internal static event Action actionSave;

        internal static readonly List<WindowRectUtill> myWindowRects = new List<WindowRectUtill>();

        internal static void ActionSave()
        {
            actionSave();
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
                windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + windowSpace, Screen.width - windowSpace);
                windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + windowSpace, Screen.height - windowSpace);
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
                windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + windowSpace, Screen.width - windowSpace);
            }
        }

        public float Y
        {
            get => windowRect.y;
            set
            {
                windowRect.y = value;
                windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + windowSpace, Screen.height - windowSpace);
            }
        }

        public Size WindowRectOpen { get => windowRectOpen; }

        public Size WindowRectClose { get => windowRectClose; }

        public int winNum;
        public static int winCnt;

        public WindowRectUtill(ConfigFile config,
                               ManualLogSource logger,
                               string fileName,
                               string windowFullName,
                               string windowShortName,
                               float wc = 100f,
                               float wo = 300f,
                               float hc = 30f,
                               float ho = 600f,
                               float x = 30f,
                               float y = 30f,
                               float windowSpace = 30f)
        {
            log = logger;

            isOpen = config.Bind("GUI", "isOpen", true);
            isGUIOn = config.Bind("GUI", "isGUIOn", false);
            isLog = config.Bind("GUI", "isLog", false);

            this.x = config.Bind("GUI", "x", x);
            this.y = config.Bind("GUI", "y", y);

            windowName = windowFullName;
            FullName = windowFullName;
            ShortName = windowShortName;

            this.windowSpace = windowSpace;

            windowRect = new Rect(x, y, wo, ho);
            windowRectOpen = new Size(wo, ho);
            windowRectClose = new Size(wc, hc);

            //jsonPath = Path.GetDirectoryName(config.ConfigFilePath) + $@"\{fileName}-rect.json";

            IsOpen = !(IsOpen = !isOpen.Value);

            actionSave += save;
            actionScreen += ScreenChg;

            winNum = winCnt++;
            load();

            myWindowRects.Add(this);
        }

        public WindowRectUtill(ConfigFile config,
                               string fileName,
                               string windowFullName,
                               string windowShortName,
                               float wc = 100f,
                               float wo = 300f,
                               float hc = 30f,
                               float ho = 600f,
                               float x = 30f,
                               float y = 30f,
                               float windowSpace = 30f) :this(config,
                                                              WindowRectGUI.log,
                                                              fileName,
                                                              windowFullName,
                                                              windowShortName,
                                                              wc,
                                                              wo,
                                                              hc,
                                                              ho,
                                                              x,
                                                              y,
                                                              windowSpace)
        {
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
                if (log ==null)
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

        [HarmonyPatch(typeof(Screen), "SetResolution", typeof(int), typeof(int), typeof(bool))]
        [HarmonyPostfix]
        private static void SetResolutionPost(int width, int height, bool fullscreen)
        {
            actionScreen(width, height);
            widthbak = width;
            heightbak = height;
            //MyLog.LogMessage("SetResolution");
            Debug.Log($"{width} , {height} , {fullscreen}");
        }

        /// <summary>
        /// 우측 정렬
        /// </summary>
        public static void GUISortIsOpen()
        {
            int i = 0, x = vGUISortX.Value, y = vGUISortY.Value, w = vGUISortDW.Value, h = vGUISortDH.Value;
            foreach (var item in myWindowRects)
            {
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
                item.X = Screen.width - x - item.Width - w * i;
                item.Y = y + h * i++;
            }
        }

        public static void GUIMinAll()
        {
            foreach (var item in myWindowRects)
            {
                item.IsOpen = false;
            }
        }

        public static void GUICloseAll()
        {
            foreach (var item in myWindowRects)
            {
                item.IsGUIOn = false;
            }
        }



    }
}
