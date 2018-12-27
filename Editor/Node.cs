﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI.Editor {
    public class NodeContext {
        public EditorViewState viewState;
        public System.Action<Port> OnClickInPoint;
        public System.Action<Port> OnClickOutPoint;
    }

    public abstract class Node {
        public Rect rect;
        public string title;
        public bool isDragged;

        List<Port> inPorts = new List<Port>();
        List<Port> outPorts = new List<Port>();

        ScriptableObject ownedScriptableObject;

        NodeContext context;

        GUIStyle boxStyle;
        Vector2 contentScrollPosition;

        public Node(string title, NodeContext context, ScriptableObject ownedScriptableObject) {
            rect = new Rect(512, 256, 400, 200);
            this.context = context;
            this.title = title;
            this.ownedScriptableObject = ownedScriptableObject;

            LoadFromViewState();
        }

        public void Drag(Vector2 delta) {
            rect.position += delta;
            SaveToViewState();
        }

        public void Draw() {
            if (boxStyle == null) {
                boxStyle = new GUIStyle(GUI.skin.box) {
                    fontSize = 18,
                    wordWrap = false,
                };
            }

            GUI.Box(rect, title, boxStyle);

            var titleHeight = 30;

            float inHeight = titleHeight, outHeight = titleHeight;
            for (int i = 0; i < inPorts.Count; ++i) {
                var port = inPorts[i];
                port.Draw(ref inHeight);
            }
            for (int i = 0; i < outPorts.Count; ++i) {
                var port = outPorts[i];
                port.Draw(ref outHeight);
            }

            var padding = 4;
            var height = Mathf.Max(inHeight, outHeight);

            GUILayout.BeginArea(new Rect(rect.x + padding, rect.y + height + padding, rect.width - padding * 2, rect.height - height - padding * 2));
            contentScrollPosition = GUILayout.BeginScrollView(contentScrollPosition);
            DrawContent();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        protected abstract void DrawContent();

        public bool ProcessEvents(Event e) {
            switch (e.type) {
                case EventType.MouseDown:
                    if (e.button == 0 && rect.Contains(e.mousePosition)) {
                        isDragged = true;
                    }
                    break;

                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged) {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }

        protected Port AddPort(PortType type, string text) {
            var point = new Port(this, type, text, type == PortType.In ? context.OnClickInPoint : context.OnClickOutPoint);
            (type == PortType.In ? inPorts : outPorts).Add(point);

            return point;
        }

        void LoadFromViewState() {
            if (context.viewState == null)
                return;

            Vector2 pos;
            if (context.viewState.TryGet(ownedScriptableObject, out pos)) {
                rect.position = pos;
            }
        }

        void SaveToViewState() {
            if (context.viewState == null) {
                context.viewState = new EditorViewState();
            }
            context.viewState.Set(ownedScriptableObject, rect.position);
        }

        Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}