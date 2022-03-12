using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using COM3D2API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LillyUtill.MyWindowRect
{
    class MyAttribute
    {
        public const string PLAGIN_NAME = "WindowRectUtill";
        public const string PLAGIN_VERSION = "22.2.26";
        public const string PLAGIN_FULL_NAME = "COM3D2.WindowRectUtill.Plugin";
    }

    [BepInPlugin(MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_NAME, MyAttribute.PLAGIN_VERSION)]
    internal class WindowRectGUI : BaseUnityPlugin
    {
        private static Harmony harmony;
        private static WindowRectUtill myWindowRect;
        internal static ManualLogSource log;

        private ConfigEntry<KeyboardShortcut> sortKey;


        WindowRectGUI()
        {
            log = Logger;            
        }

        private void Awake()
        {
            log.LogMessage("Awake");
            log.LogMessage("https://github.com/customordermaid3d2/COM3D2.WindowRectUtill");

            WindowRectUtill.widthbak = Screen.width;
            WindowRectUtill.heightbak = Screen.height;

            WindowRectUtill.init(Config);

            if (harmony == null)
            {
                harmony = Harmony.CreateAndPatchAll(typeof(WindowRectUtill));
            }

            SceneManager.sceneLoaded += this.OnSceneLoaded;

            myWindowRect=new WindowRectUtill( Config, MyAttribute.PLAGIN_FULL_NAME, MyAttribute.PLAGIN_NAME,"WR");

            sortKey = Config.Bind("GUI", "sort key",new KeyboardShortcut(KeyCode.BackQuote, KeyCode.LeftAlt ));
        }



        private void Start()
        {
            WindowRectUtill.widthbak = Screen.width;
            WindowRectUtill.heightbak = Screen.height;

            SystemShortcutAPI.AddButton(
MyAttribute.PLAGIN_FULL_NAME
, new Action(delegate ()
{ // 기어메뉴 아이콘 클릭시 작동할 기능
                myWindowRect.IsGUIOn = !myWindowRect.IsGUIOn;
})
, MyAttribute.PLAGIN_NAME // 표시될 툴팁 내용                               
, Properties.Resources.icon);// 표시될 아이콘
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "SceneADV")
                WindowRectUtill.ActionSave();
        }

        private void Update()
        {
            if (sortKey.Value.IsDown())
            {
                log.LogMessage("Update.GUISortAll");
                WindowRectUtill.GUISortAll();
            }            
        }

        private void OnGUI()
        {
            if (!myWindowRect.IsGUIOn)
                return;

            //GUI.skin.window = ;

            //myWindowRect.WindowRect = GUILayout.Window(windowId, myWindowRect.WindowRect, WindowFunction, MyAttribute.PLAGIN_NAME + " " + ShowCounter.Value.ToString(), GUI.skin.box);
            // 별도 창을 띄우고 WindowFunction 를 실행함. 이건 스킨 설정 부분인데 따로 공부할것
            myWindowRect.WindowRect = GUILayout.Window(myWindowRect.winNum, myWindowRect.WindowRect, WindowFunction, "", GUI.skin.box);
        }

        private Vector2 scrollPosition;

        private void WindowFunction(int id)
        {
            GUI.enabled = true; // 기능 클릭 가능

            GUILayout.BeginHorizontal();// 가로 정렬
            // 라벨 추가
            GUILayout.Label(myWindowRect.windowName, GUILayout.Height(20));
            // 안쓰는 공간이 생기더라도 다른 기능으로 꽉 채우지 않고 빈공간 만들기
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { myWindowRect.IsOpen = !myWindowRect.IsOpen; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { myWindowRect.IsGUIOn = false; }
            GUI.changed = false;

            GUILayout.EndHorizontal();// 가로 정렬 끝

            if (!myWindowRect.IsOpen)
            {

            }
            else
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

                #region 여기에 내용 작성

                GUILayout.Label($"{sortKey.Value.IsDown()} {sortKey.Value.IsPressed()} {sortKey.Value.IsUp()}");
                if (GUILayout.Button("GUICloseAll", GUILayout.Height(20))) { WindowRectUtill.GUICloseAll(); }
                if (GUILayout.Button("GUIMinAll", GUILayout.Height(20))) { WindowRectUtill.GUIMinAll(); }
                if (GUILayout.Button($"GUISortAll {sortKey.Value.ToString()}", GUILayout.Height(20))) { WindowRectUtill.GUISortAll(); }
                if (GUILayout.Button("GUISortOn", GUILayout.Height(20))) { WindowRectUtill.GUISortIsGUIOn(); }
                if (GUILayout.Button("GUISortOpen", GUILayout.Height(20))) { WindowRectUtill.GUISortIsOpen(); }
                
                foreach (var item in WindowRectUtill.myWindowRects)
                {                    
                    GUILayout.Label($"{item.winNum} {item.ShortName} {item.FullName}");
                }

                #endregion

                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
        }


        private void OnApplicationQuit()
        {
            WindowRectUtill.ActionSave();
        }
    }
}
