using UnityEngine;

namespace Kamgam.UGUIComponentsForSettings
{
    public static partial class InputUtils
    {
        // For the NEW InputSystem implementation see
        // InputUtils.InputSystem.cs

#if !ENABLE_INPUT_SYSTEM

        public static KeyCode[] KeyCodes;

        static void buildKeyCodeCache()
        {
            if (KeyCodes == null)
            {
                KeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
            }
        }

        public static bool AnyKey()
        {
            return Input.anyKey || InputAxisButtonEvents.AnyButton();
        }

        /// <summary>
        /// Returns true the first frame the user hits any key or mouse button.
        /// </summary>
        /// <returns></returns>
        public static bool AnyKeyDown()
        {
            return Input.anyKeyDown || InputAxisButtonEvents.AnyButtonDown();
        }

        /// <summary>
        /// Returns true during the frame the user releases any mouse button.
        /// </summary>
        /// <returns></returns>
        public static bool MouseUp()
        {
            return
               Input.GetMouseButtonUp(0)
            || Input.GetMouseButtonUp(1)
            || Input.GetMouseButtonUp(2)
            || Input.GetMouseButtonUp(3)
            || Input.GetMouseButtonUp(4)
            || Input.GetMouseButtonUp(5)
            || Input.GetMouseButtonUp(6);
        }

        public static bool SubmitDown()
        {
            return Input.GetButtonDown("Submit");
        }

        public static bool SubmitUp()
        {
            return Input.GetButtonUp("Submit");
        }

        public static bool CancelDown()
        {
            return Input.GetButtonDown("Cancel");
        }

        public static bool CancelUp()
        {
            return Input.GetButtonUp("Cancel");
        }

        public static bool AnyDirection()
        {
            return Input.GetButton("Horizontal")
                || Input.GetButton("Vertical")
                || !Mathf.Approximately(Input.GetAxis("Horizontal"), 0)
                || !Mathf.Approximately(Input.GetAxis("Vertical"), 0);
        }

        public static bool LeftMouse()
        {
            return Input.GetKey(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyUp(KeyCode.Mouse0);
        }

        /// <summary>
        /// Returns true during the frame the user starts pressing any modifier key (Shift, Tab, Control, Command or Alt).
        /// </summary>
        /// <returns></returns>
        public static UniversalKeyCode GetModifierKeyDown()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) return UniversalKeyCode.LeftShift;
            if (Input.GetKeyDown(KeyCode.RightShift)) return UniversalKeyCode.RightShift;
            if (Input.GetKeyDown(KeyCode.Tab)) return UniversalKeyCode.Tab;
            if (Input.GetKeyDown(KeyCode.LeftControl)) return UniversalKeyCode.LeftControl;
            if (Input.GetKeyDown(KeyCode.RightControl)) return UniversalKeyCode.RightControl;
            if (Input.GetKeyDown(KeyCode.LeftCommand)) return UniversalKeyCode.LeftCommand;
            if (Input.GetKeyDown(KeyCode.RightCommand)) return UniversalKeyCode.RightCommand;
            if (Input.GetKeyDown(KeyCode.LeftAlt)) return UniversalKeyCode.LeftAlt;
            if (Input.GetKeyDown(KeyCode.RightAlt)) return UniversalKeyCode.RightAlt;

            return UniversalKeyCode.None;
        }

        /// <summary>
        /// Get the UniversalKeyCode for the key/mouse/button that was pressed down within the last frame.
        /// </summary>
        /// <param name="excludeModifierKeys"></param>
        /// <param name="excludeMouseButtons"></param>
        /// <returns></returns>
        public static UniversalKeyCode GetUniversalKeyDown(bool excludeModifierKeys, bool excludeMouseButtons)
        {
            buildKeyCodeCache();
            foreach (KeyCode keyCode in KeyCodes)
            {
                if (excludeModifierKeys)
                {
                    if (keyCode == KeyCode.LeftShift) continue;
                    if (keyCode == KeyCode.RightShift) continue;
                    if (keyCode == KeyCode.Tab) continue;
                    if (keyCode == KeyCode.LeftControl) continue;
                    if (keyCode == KeyCode.RightControl) continue;
                    if (keyCode == KeyCode.LeftCommand) continue;
                    if (keyCode == KeyCode.RightCommand) continue;
                    if (keyCode == KeyCode.LeftAlt) continue;
                    if (keyCode == KeyCode.RightAlt) continue;
                }

                if (excludeMouseButtons)
                {
                    if (keyCode == KeyCode.Mouse0) continue;
                    if (keyCode == KeyCode.Mouse1) continue;
                    if (keyCode == KeyCode.Mouse2) continue;
                    if (keyCode == KeyCode.Mouse3) continue;
                    if (keyCode == KeyCode.Mouse4) continue;
                    if (keyCode == KeyCode.Mouse5) continue;
                    if (keyCode == KeyCode.Mouse6) continue;
                }

                if (Input.GetKeyDown(keyCode))
                {
                    var universalKeyCode = KeyCodeToUniversalKeyCode(keyCode, convertJoyStickToGamePad: true);
                    return universalKeyCode;
                }
            }

            if(InputAxisButtonEvents.AnyButtonDown())
            {
                return InputAxisButtonEvents.FirstButtonDown();
            }
            
            return UniversalKeyCode.None;
        }

        /// <summary>
        /// Get the UniversalKeyCode for the key/mouse/button that was pressed down within the last frame.
        /// </summary>
        /// <param name="excludeModifierKeys"></param>
        /// <param name="excludeMouseButtons"></param>
        /// <returns></returns>
        public static UniversalKeyCode GetUniversalKeyUp(bool excludeModifierKeys, bool excludeMouseButtons)
        {
            buildKeyCodeCache();
            foreach (KeyCode keyCode in KeyCodes)
            {
                if (excludeModifierKeys)
                {
                    if (keyCode == KeyCode.LeftShift) continue;
                    if (keyCode == KeyCode.RightShift) continue;
                    if (keyCode == KeyCode.Tab) continue;
                    if (keyCode == KeyCode.LeftControl) continue;
                    if (keyCode == KeyCode.RightControl) continue;
                    if (keyCode == KeyCode.LeftCommand) continue;
                    if (keyCode == KeyCode.RightCommand) continue;
                    if (keyCode == KeyCode.LeftAlt) continue;
                    if (keyCode == KeyCode.RightAlt) continue;
                }

                if (excludeMouseButtons)
                {
                    if (keyCode == KeyCode.Mouse0) continue;
                    if (keyCode == KeyCode.Mouse1) continue;
                    if (keyCode == KeyCode.Mouse2) continue;
                    if (keyCode == KeyCode.Mouse3) continue;
                    if (keyCode == KeyCode.Mouse4) continue;
                    if (keyCode == KeyCode.Mouse5) continue;
                    if (keyCode == KeyCode.Mouse6) continue;
                }

                if (Input.GetKeyUp(keyCode))
                {
                    var universalKeyCode = KeyCodeToUniversalKeyCode(keyCode, convertJoyStickToGamePad: true);
                    return universalKeyCode;
                }

                if (InputAxisButtonEvents.AnyButtonUp())
                {
                    return InputAxisButtonEvents.FirstButtonUp();
                }
            }

            return UniversalKeyCode.None;
        }


        /// <summary>
        /// Retuns a human readable key name (it aims to return the text which is printed on the keyboard keys).
        /// Key codes are somewhat layout dependent (Z and Y switch correctly for EN / DE) but others don't, צהü for example.
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="richText">Replace some keys with sprites?</param>
        /// <returns></returns>
        public static string UniversalKeyName(UniversalKeyCode keyCode)
        {
            switch (keyCode)
            {
                case UniversalKeyCode.None: return "-";

                case UniversalKeyCode.MouseLeft: return "Left Mouse";
                case UniversalKeyCode.MouseRight: return "Right Mouse";
                case UniversalKeyCode.MouseMiddle: return "Middle Mouse";
                case UniversalKeyCode.MouseBack: return "Back Mouse";
                case UniversalKeyCode.MouseForward: return "Forward Mouse";

                case UniversalKeyCode.Backspace: return "Backspace";
                case UniversalKeyCode.Tab: return "Tab";
                case UniversalKeyCode.Clear: return "Clear";
                case UniversalKeyCode.Return: return "Return";
                case UniversalKeyCode.Pause: return "Pause";
                case UniversalKeyCode.Escape: return "Esc";
                case UniversalKeyCode.Space: return "Space";
                case UniversalKeyCode.Exclaim: return "!";
                case UniversalKeyCode.DoubleQuote: return "\"";
                case UniversalKeyCode.Hash: return "#";
                case UniversalKeyCode.Dollar: return "$";
                case UniversalKeyCode.Percent: return "%";
                case UniversalKeyCode.Ampersand: return "&";
                case UniversalKeyCode.Quote: return "'";
                case UniversalKeyCode.LeftParen: return "(";
                case UniversalKeyCode.RightParen: return ")";
                case UniversalKeyCode.Asterisk: return "*";
                case UniversalKeyCode.Plus: return "+";
                case UniversalKeyCode.Comma: return ",";
                case UniversalKeyCode.Minus: return "-";
                case UniversalKeyCode.Period: return ".";
                case UniversalKeyCode.Slash: return "/";
                case UniversalKeyCode.Digit0: return "0";
                case UniversalKeyCode.Digit1: return "1";
                case UniversalKeyCode.Digit2: return "2";
                case UniversalKeyCode.Digit3: return "3";
                case UniversalKeyCode.Digit4: return "4";
                case UniversalKeyCode.Digit5: return "5";
                case UniversalKeyCode.Digit6: return "6";
                case UniversalKeyCode.Digit7: return "7";
                case UniversalKeyCode.Digit8: return "8";
                case UniversalKeyCode.Digit9: return "9";
                case UniversalKeyCode.Colon: return ":";
                case UniversalKeyCode.Semicolon: return ";";
                case UniversalKeyCode.Less: return "<";
                case UniversalKeyCode.Equals: return "=";
                case UniversalKeyCode.Greater: return ">";
                case UniversalKeyCode.Question: return "?";
                case UniversalKeyCode.At: return "@";
                case UniversalKeyCode.LeftBracket: return "[";
                case UniversalKeyCode.Backslash: return "\\";
                case UniversalKeyCode.RightBracket: return "]";
                case UniversalKeyCode.Caret: return "^";
                case UniversalKeyCode.Underscore: return "_";
                case UniversalKeyCode.BackQuote: return "`";
                case UniversalKeyCode.A: return "A";
                case UniversalKeyCode.B: return "B";
                case UniversalKeyCode.C: return "C";
                case UniversalKeyCode.D: return "D";
                case UniversalKeyCode.E: return "E";
                case UniversalKeyCode.F: return "F";
                case UniversalKeyCode.G: return "G";
                case UniversalKeyCode.H: return "H";
                case UniversalKeyCode.I: return "I";
                case UniversalKeyCode.J: return "J";
                case UniversalKeyCode.K: return "K";
                case UniversalKeyCode.L: return "L";
                case UniversalKeyCode.M: return "M";
                case UniversalKeyCode.N: return "N";
                case UniversalKeyCode.O: return "O";
                case UniversalKeyCode.P: return "P";
                case UniversalKeyCode.Q: return "Q";
                case UniversalKeyCode.R: return "R";
                case UniversalKeyCode.S: return "S";
                case UniversalKeyCode.T: return "T";
                case UniversalKeyCode.U: return "U";
                case UniversalKeyCode.V: return "V";
                case UniversalKeyCode.W: return "W";
                case UniversalKeyCode.X: return "X";
                case UniversalKeyCode.Y: return "Y";
                case UniversalKeyCode.Z: return "Z";
                case UniversalKeyCode.LeftCurlyBracket: return "{";
                case UniversalKeyCode.Pipe: return "|";
                case UniversalKeyCode.RightCurlyBracket: return "}";
                case UniversalKeyCode.Tilde: return "~";
                case UniversalKeyCode.Delete: return "Del";
                case UniversalKeyCode.Numpad0: return "0";
                case UniversalKeyCode.Numpad1: return "1";
                case UniversalKeyCode.Numpad2: return "2";
                case UniversalKeyCode.Numpad3: return "3";
                case UniversalKeyCode.Numpad4: return "4";
                case UniversalKeyCode.Numpad5: return "5";
                case UniversalKeyCode.Numpad6: return "6";
                case UniversalKeyCode.Numpad7: return "7";
                case UniversalKeyCode.Numpad8: return "8";
                case UniversalKeyCode.Numpad9: return "9";
                case UniversalKeyCode.NumpadPeriod: return ".";
                case UniversalKeyCode.NumpadDivide: return "/";
                case UniversalKeyCode.NumpadMultiply: return "*";
                case UniversalKeyCode.NumpadMinus: return "-";
                case UniversalKeyCode.NumpadPlus: return "+";
                case UniversalKeyCode.NumpadEnter: return "Enter";
                case UniversalKeyCode.NumpadEquals: return "=";
                case UniversalKeyCode.UpArrow: return "Up Arrow";
                case UniversalKeyCode.DownArrow: return "Down Arrow";
                case UniversalKeyCode.RightArrow: return "Right Arrow";
                case UniversalKeyCode.LeftArrow: return "Left Arrow";
                case UniversalKeyCode.Insert: return "Insert";
                case UniversalKeyCode.Home: return "Home";
                case UniversalKeyCode.End: return "End";
                case UniversalKeyCode.PageUp: return "Page Up";
                case UniversalKeyCode.PageDown: return "Page Down";
                case UniversalKeyCode.F1: return "F1";
                case UniversalKeyCode.F2: return "F2";
                case UniversalKeyCode.F3: return "F3";
                case UniversalKeyCode.F4: return "F4";
                case UniversalKeyCode.F5: return "F5";
                case UniversalKeyCode.F6: return "F6";
                case UniversalKeyCode.F7: return "F7";
                case UniversalKeyCode.F8: return "F8";
                case UniversalKeyCode.F9: return "F9";
                case UniversalKeyCode.F10: return "F10";
                case UniversalKeyCode.F11: return "F11";
                case UniversalKeyCode.F12: return "F12";
                case UniversalKeyCode.NumLock: return "NumLock";
                case UniversalKeyCode.CapsLock: return "CapsLock";
                case UniversalKeyCode.ScrollLock: return "ScrollLock";
                case UniversalKeyCode.RightShift: return "Shift";
                case UniversalKeyCode.LeftShift: return "Shift";
                case UniversalKeyCode.RightControl: return "Ctrl";
                case UniversalKeyCode.LeftControl: return "Ctrl";
                case UniversalKeyCode.RightAlt: return "Alt";
                case UniversalKeyCode.LeftAlt: return "Alt";
                case UniversalKeyCode.RightCommand: return "Cmd";
                case UniversalKeyCode.LeftCommand: return "Cmd";
                case UniversalKeyCode.LeftWindows: return "Win";
                case UniversalKeyCode.RightWindows: return "RightWindows";
                case UniversalKeyCode.AltGr: return "AltGr";
                case UniversalKeyCode.Help: return "Help";
                case UniversalKeyCode.Print: return "Print";
                case UniversalKeyCode.SysReq: return "SysReq";
                case UniversalKeyCode.Break: return "Break";
                case UniversalKeyCode.Menu: return "Menu";

                case UniversalKeyCode.JoystickButton0: return "Joy0";
                case UniversalKeyCode.JoystickButton1: return "Joy1";
                case UniversalKeyCode.JoystickButton2: return "Joy2";
                case UniversalKeyCode.JoystickButton3: return "Joy3";
                case UniversalKeyCode.JoystickButton4: return "Joy4";
                case UniversalKeyCode.JoystickButton5: return "Joy5";
                case UniversalKeyCode.JoystickButton6: return "Joy6";
                case UniversalKeyCode.JoystickButton7: return "Joy7";
                case UniversalKeyCode.JoystickButton8: return "Joy8";
                case UniversalKeyCode.JoystickButton9: return "Joy9";
                case UniversalKeyCode.JoystickButton10: return "Joy10";
                case UniversalKeyCode.JoystickButton11: return "Joy11";
                case UniversalKeyCode.JoystickButton12: return "Joy12";
                case UniversalKeyCode.JoystickButton13: return "Joy13";
                case UniversalKeyCode.JoystickButton14: return "Joy14";
                case UniversalKeyCode.JoystickButton15: return "Joy15";
                case UniversalKeyCode.JoystickButton16: return "Joy16";
                case UniversalKeyCode.JoystickButton17: return "Joy17";
                case UniversalKeyCode.JoystickButton18: return "Joy18";
                case UniversalKeyCode.JoystickButton19: return "Joy19";

                case UniversalKeyCode.GamePadNorth: return "Y";
                case UniversalKeyCode.GamePadSouth: return "A";
                case UniversalKeyCode.GamePadWest: return "X";
                case UniversalKeyCode.GamePadEast: return "B";
                case UniversalKeyCode.GamePadStart: return "Start";
                case UniversalKeyCode.GamePadSelect: return "Select";
                case UniversalKeyCode.GamePadLeftShoulder: return "LB";
                case UniversalKeyCode.GamePadRightShoulder: return "RB";
                case UniversalKeyCode.GamePadLeftTrigger: return "LT";
                case UniversalKeyCode.GamePadRightTrigger: return "RT";
                case UniversalKeyCode.GamePadDPadUp: return "DPad Up";
                case UniversalKeyCode.GamePadDPadDown: return "DPad Down";
                case UniversalKeyCode.GamePadDPadLeft: return "DPad Left";
                case UniversalKeyCode.GamePadDPadRight: return "DPad Right";
                case UniversalKeyCode.GamePadLeftStickButton: return "Left Stick";
                case UniversalKeyCode.GamePadRightStickButton: return "Right Stick";

                case UniversalKeyCode.GamePadLeftStickUp: return "L-Stick Up";
                case UniversalKeyCode.GamePadLeftStickDown: return "L-Stick Down";
                case UniversalKeyCode.GamePadLeftStickLeft: return "L-Stick Left";
                case UniversalKeyCode.GamePadLeftStickRight: return "L-Stick Right";

                case UniversalKeyCode.GamePadRightStickUp: return "R-Stick Up";
                case UniversalKeyCode.GamePadRightStickDown: return "R-Stick Down";
                case UniversalKeyCode.GamePadRightStickLeft: return "R-Stick Left";
                case UniversalKeyCode.GamePadRightStickRight: return "R-Stick Right";

                default:
                    return keyCode.ToString();
            }
        }

#endif

        /// <summary>
        /// Converts the KeyCode into universal key codes.
        /// <br />
        /// Uses the Xbox Controller mapping found here:<br />
        /// https://answers.unity.com/storage/temp/134371-xbox-one-controller-unity-windows-macos.jpg
        /// <br />
        /// If you want a more sophisticated controller detectsion then we'd recommend using In
        /// the new input system or InControl:<br />
        /// https://assetstore.unity.com/packages/tools/input-management/incontrol-14695?aid=1100lqC54
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        public static UniversalKeyCode KeyCodeToUniversalKeyCode(KeyCode keyCode)
        {
            return KeyCodeToUniversalKeyCode(keyCode, convertJoyStickToGamePad: true);
        }

        public static UniversalKeyCode KeyCodeToUniversalKeyCode(KeyCode keyCode, bool convertJoyStickToGamePad)
        {
            switch (keyCode)
            {
                case KeyCode.None: return UniversalKeyCode.None;
                case KeyCode.Backspace: return UniversalKeyCode.Backspace;
                case KeyCode.Delete: return UniversalKeyCode.Delete;
                case KeyCode.Tab: return UniversalKeyCode.Tab;
                case KeyCode.Clear: return UniversalKeyCode.Clear;
                case KeyCode.Return: return UniversalKeyCode.Return;
                case KeyCode.Pause: return UniversalKeyCode.Pause;
                case KeyCode.Escape: return UniversalKeyCode.Escape;
                case KeyCode.Space: return UniversalKeyCode.Space;
                case KeyCode.Keypad0: return UniversalKeyCode.Numpad0;
                case KeyCode.Keypad1: return UniversalKeyCode.Numpad1;
                case KeyCode.Keypad2: return UniversalKeyCode.Numpad2;
                case KeyCode.Keypad3: return UniversalKeyCode.Numpad3;
                case KeyCode.Keypad4: return UniversalKeyCode.Numpad4;
                case KeyCode.Keypad5: return UniversalKeyCode.Numpad5;
                case KeyCode.Keypad6: return UniversalKeyCode.Numpad6;
                case KeyCode.Keypad7: return UniversalKeyCode.Numpad7;
                case KeyCode.Keypad8: return UniversalKeyCode.Numpad8;
                case KeyCode.Keypad9: return UniversalKeyCode.Numpad9;
                case KeyCode.KeypadPeriod: return UniversalKeyCode.NumpadPeriod;
                case KeyCode.KeypadDivide: return UniversalKeyCode.NumpadDivide;
                case KeyCode.KeypadMultiply: return UniversalKeyCode.NumpadMultiply;
                case KeyCode.KeypadMinus: return UniversalKeyCode.NumpadMinus;
                case KeyCode.KeypadPlus: return UniversalKeyCode.NumpadPlus;
                case KeyCode.KeypadEnter: return UniversalKeyCode.NumpadEnter;
                case KeyCode.KeypadEquals: return UniversalKeyCode.NumpadEquals;
                case KeyCode.UpArrow: return UniversalKeyCode.UpArrow;
                case KeyCode.DownArrow: return UniversalKeyCode.DownArrow;
                case KeyCode.RightArrow: return UniversalKeyCode.RightArrow;
                case KeyCode.LeftArrow: return UniversalKeyCode.LeftArrow;
                case KeyCode.Insert: return UniversalKeyCode.Insert;
                case KeyCode.Home: return UniversalKeyCode.Home;
                case KeyCode.End: return UniversalKeyCode.End;
                case KeyCode.PageUp: return UniversalKeyCode.PageUp;
                case KeyCode.PageDown: return UniversalKeyCode.PageDown;
                case KeyCode.F1: return UniversalKeyCode.F1;
                case KeyCode.F2: return UniversalKeyCode.F2;
                case KeyCode.F3: return UniversalKeyCode.F3;
                case KeyCode.F4: return UniversalKeyCode.F4;
                case KeyCode.F5: return UniversalKeyCode.F5;
                case KeyCode.F6: return UniversalKeyCode.F6;
                case KeyCode.F7: return UniversalKeyCode.F7;
                case KeyCode.F8: return UniversalKeyCode.F8;
                case KeyCode.F9: return UniversalKeyCode.F9;
                case KeyCode.F10: return UniversalKeyCode.F10;
                case KeyCode.F11: return UniversalKeyCode.F11;
                case KeyCode.F12: return UniversalKeyCode.F12;
                case KeyCode.F13: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.F14: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.F15: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Alpha0: return UniversalKeyCode.Digit0;
                case KeyCode.Alpha1: return UniversalKeyCode.Digit1;
                case KeyCode.Alpha2: return UniversalKeyCode.Digit2;
                case KeyCode.Alpha3: return UniversalKeyCode.Digit3;
                case KeyCode.Alpha4: return UniversalKeyCode.Digit4;
                case KeyCode.Alpha5: return UniversalKeyCode.Digit5;
                case KeyCode.Alpha6: return UniversalKeyCode.Digit6;
                case KeyCode.Alpha7: return UniversalKeyCode.Digit7;
                case KeyCode.Alpha8: return UniversalKeyCode.Digit8;
                case KeyCode.Alpha9: return UniversalKeyCode.Digit9;
                case KeyCode.Exclaim: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.DoubleQuote: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Hash: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Dollar: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Percent: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Ampersand: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Quote: return UniversalKeyCode.Quote;
                case KeyCode.LeftParen: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.RightParen: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Asterisk: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Plus: return UniversalKeyCode.Plus;
                case KeyCode.Comma: return UniversalKeyCode.Comma;
                case KeyCode.Minus: return UniversalKeyCode.Minus;
                case KeyCode.Period: return UniversalKeyCode.Period;
                case KeyCode.Slash: return UniversalKeyCode.Slash;
                case KeyCode.Colon: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Semicolon: return UniversalKeyCode.Semicolon;
                case KeyCode.Less: return UniversalKeyCode.None;
                case KeyCode.Equals: return UniversalKeyCode.Equals;
                case KeyCode.Greater: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Question: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.At: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.LeftBracket: return UniversalKeyCode.LeftBracket;
                case KeyCode.Backslash: return UniversalKeyCode.Backslash;
                case KeyCode.RightBracket: return UniversalKeyCode.RightBracket;
                case KeyCode.Caret: return UniversalKeyCode.None; // TODO
                case KeyCode.Underscore: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.BackQuote: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.A: return UniversalKeyCode.A;
                case KeyCode.B: return UniversalKeyCode.B;
                case KeyCode.C: return UniversalKeyCode.C;
                case KeyCode.D: return UniversalKeyCode.D;
                case KeyCode.E: return UniversalKeyCode.E;
                case KeyCode.F: return UniversalKeyCode.F;
                case KeyCode.G: return UniversalKeyCode.G;
                case KeyCode.H: return UniversalKeyCode.H;
                case KeyCode.I: return UniversalKeyCode.I;
                case KeyCode.J: return UniversalKeyCode.J;
                case KeyCode.K: return UniversalKeyCode.K;
                case KeyCode.L: return UniversalKeyCode.L;
                case KeyCode.M: return UniversalKeyCode.M;
                case KeyCode.N: return UniversalKeyCode.N;
                case KeyCode.O: return UniversalKeyCode.O;
                case KeyCode.P: return UniversalKeyCode.P;
                case KeyCode.Q: return UniversalKeyCode.Q;
                case KeyCode.R: return UniversalKeyCode.R;
                case KeyCode.S: return UniversalKeyCode.S;
                case KeyCode.T: return UniversalKeyCode.T;
                case KeyCode.U: return UniversalKeyCode.U;
                case KeyCode.V: return UniversalKeyCode.V;
                case KeyCode.W: return UniversalKeyCode.W;
                case KeyCode.X: return UniversalKeyCode.X;
                case KeyCode.Y: return UniversalKeyCode.Y;
                case KeyCode.Z: return UniversalKeyCode.Z;
                case KeyCode.LeftCurlyBracket: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Pipe: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.RightCurlyBracket: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Tilde: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Numlock: return UniversalKeyCode.NumLock;
                case KeyCode.CapsLock: return UniversalKeyCode.CapsLock;
                case KeyCode.ScrollLock: return UniversalKeyCode.ScrollLock;
                case KeyCode.RightShift: return UniversalKeyCode.RightShift;
                case KeyCode.LeftShift: return UniversalKeyCode.LeftShift;
                case KeyCode.RightControl: return UniversalKeyCode.RightControl;
                case KeyCode.LeftControl: return UniversalKeyCode.LeftControl;
                case KeyCode.RightAlt: return UniversalKeyCode.RightAlt;
                case KeyCode.LeftAlt: return UniversalKeyCode.LeftAlt;
                case KeyCode.LeftCommand: return UniversalKeyCode.LeftCommand;
                // case KeyCode.LeftApple: (same as LeftCommand)
                case KeyCode.LeftWindows: return UniversalKeyCode.LeftWindows;
                case KeyCode.RightCommand: return UniversalKeyCode.RightCommand;
                // case KeyCode.RightApple: (same as RightCommand)
                case KeyCode.RightWindows: return UniversalKeyCode.RightWindows;
                case KeyCode.AltGr: return UniversalKeyCode.AltGr;
                case KeyCode.Help: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Print: return UniversalKeyCode.Print;
                case KeyCode.SysReq: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Break: return UniversalKeyCode.Unknown; // Not Supported
                case KeyCode.Menu: return UniversalKeyCode.Menu;

                case KeyCode.Mouse0: return UniversalKeyCode.MouseLeft;
                case KeyCode.Mouse1: return UniversalKeyCode.MouseMiddle;
                case KeyCode.Mouse2: return UniversalKeyCode.MouseRight;
                case KeyCode.Mouse3: return UniversalKeyCode.MouseBack;
                case KeyCode.Mouse4: return UniversalKeyCode.MouseForward;
            }

            if (convertJoyStickToGamePad)
            {
                switch (keyCode)
                {
                    case KeyCode.JoystickButton0: return UniversalKeyCode.GamePadSouth;
                    case KeyCode.JoystickButton1: return UniversalKeyCode.GamePadEast;
                    case KeyCode.JoystickButton2: return UniversalKeyCode.GamePadWest;
                    case KeyCode.JoystickButton3: return UniversalKeyCode.GamePadNorth;
                    case KeyCode.JoystickButton4: return UniversalKeyCode.GamePadLeftShoulder;
                    case KeyCode.JoystickButton5: return UniversalKeyCode.GamePadRightShoulder;
                    case KeyCode.JoystickButton6: return UniversalKeyCode.GamePadSelect;
                    case KeyCode.JoystickButton7: return UniversalKeyCode.GamePadStart;
                    case KeyCode.JoystickButton8: return UniversalKeyCode.GamePadLeftStickButton;
                    case KeyCode.JoystickButton9: return UniversalKeyCode.GamePadRightStickButton;
                    case KeyCode.JoystickButton10: return UniversalKeyCode.JoystickButton10;
                }
            }
            else
            {
                switch (keyCode)
                {
                    case KeyCode.JoystickButton0: return UniversalKeyCode.JoystickButton0;
                    case KeyCode.JoystickButton1: return UniversalKeyCode.JoystickButton1;
                    case KeyCode.JoystickButton2: return UniversalKeyCode.JoystickButton2;
                    case KeyCode.JoystickButton3: return UniversalKeyCode.JoystickButton3;
                    case KeyCode.JoystickButton4: return UniversalKeyCode.JoystickButton4;
                    case KeyCode.JoystickButton5: return UniversalKeyCode.JoystickButton5;
                    case KeyCode.JoystickButton6: return UniversalKeyCode.JoystickButton6;
                    case KeyCode.JoystickButton7: return UniversalKeyCode.JoystickButton7;
                    case KeyCode.JoystickButton8: return UniversalKeyCode.JoystickButton8;
                    case KeyCode.JoystickButton9: return UniversalKeyCode.JoystickButton9;
                    case KeyCode.JoystickButton10: return UniversalKeyCode.JoystickButton10;
                }
            }

            return UniversalKeyCode.Unknown;
        }
    }
}
