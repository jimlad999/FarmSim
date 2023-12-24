using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Utils;

namespace UI;

public delegate void TextInputEventHandler(TextInput sender, string value);

[DataContract]
public class TextInput : UIElement
{
    private const int NormalAlpha = 255;
    private const int PlaceholderAlpha = 180;
    private const double CaretFlashTimeMax = 1.0;//second

    [DataMember]
    public string Color;
    [DataMember]
    public string Placeholder;
    // Can be set by the screen on initialization, but also updated by the user by clicking in/out of the input area while this element is active
    [DataMember]
    public bool Focus;

    [IgnoreDataMember]
    public TextInputEventHandler EventHandler;
    [IgnoreDataMember]
    private Text Text;
    [IgnoreDataMember]
    private StringBuilder Value = new();
    [IgnoreDataMember]
    private int CursorIndex = 0;
    [IgnoreDataMember]
    private List<string> History = new();
    [IgnoreDataMember]
    private int HistoryIndex = 0;
    [IgnoreDataMember]
    private bool IgnoreLastKeyPressInternal;
    [IgnoreDataMember]
    private double CaretFlashTime = CaretFlashTimeMax;
    [IgnoreDataMember]
    private bool ShowCaret = true;

    public void IgnoreLastKeyPress()
    {
        IgnoreLastKeyPressInternal = true;
    }

    public override void Update(GameTime gameTime, UIState state, UISpriteSheet uiSpriteSheet, ControllerManager controllerManager)
    {
        CaretFlashTime -= gameTime.ElapsedGameTime.TotalSeconds;
        if (CaretFlashTime < 0)
        {
            ShowCaret = !ShowCaret;
            CaretFlashTime = CaretFlashTimeMax;
        }
        if (Text == null)
        {
            Text = new Text
            {
                Id = Id,
                Top = Top,
                Left = Left,
                Bottom = Bottom,
                Right = Right,
                HorizontalAlignment = HorizontalAlignment,
                VerticalAlignment = VerticalAlignment,
                Hidden = Hidden,
                Margin = Margin,
                Value = $"<{Color}>{Placeholder}",
                AlphaModifier = PlaceholderAlpha,
            };
            Children.Add(Text);
        }
        if (IgnoreLastKeyPressInternal)
        {
            IgnoreLastKeyPressInternal = false;
            base.Update(gameTime, state, uiSpriteSheet, controllerManager);
            return;
        }
        if (controllerManager.IsKeyInitialPressed(Keys.Enter))
        {
            if (EventHandler != null && Value.Length > 0)
            {
                var value = Value.ToString();
                if (History.Count == 0 || History[^1] != value)
                {
                    History.Add(value);
                }
                HistoryIndex = 0;
                CursorIndex = 0;
                Value.Clear();
                Text.UpdateValue($"<{Color}>{Placeholder}");
                Text.AlphaModifier = PlaceholderAlpha;
                EventHandler.Invoke(this, value.Replace(Text.PlaceholderGT, '>').Replace(Text.PlaceholderLT, '<'));
            }
        }
        else if (controllerManager.IsKeyPressedWithRepeat(Keys.Back))
        {
            if (CursorIndex > 0)
            {
                Value = Value.Remove(--CursorIndex, 1);
                if (Value.Length == 0)
                {
                    Text.UpdateValue($"<{Color}>{Placeholder}");
                    Text.AlphaModifier = PlaceholderAlpha;
                }
                else
                {
                    Text.UpdateValue($"<{Color}>{Value}");
                    Text.AlphaModifier = NormalAlpha;
                }
            }
        }
        else if (controllerManager.IsKeyPressedWithRepeat(Keys.Delete))
        {
            if (Value.Length > CursorIndex)
            {
                Value = Value.Remove(CursorIndex, 1);
                if (Value.Length == 0)
                {
                    Text.UpdateValue($"<{Color}>{Placeholder}");
                    Text.AlphaModifier = PlaceholderAlpha;
                }
                else
                {
                    Text.UpdateValue($"<{Color}>{Value}");
                    Text.AlphaModifier = NormalAlpha;
                }
            }
        }
        else if (controllerManager.IsKeyPressedWithRepeat(Keys.Left))
        {
            if (CursorIndex > 0)
            {
                --CursorIndex;
            }
        }
        else if (controllerManager.IsKeyPressedWithRepeat(Keys.Right))
        {
            if (CursorIndex < Value.Length)
            {
                ++CursorIndex;
            }
        }
        else if (controllerManager.IsKeyPressedWithRepeat(Keys.Up))
        {
            if (HistoryIndex < History.Count)
            {
                var value = History[History.Count - ++HistoryIndex];
                Value.Clear();
                Value.Append(value);
                CursorIndex = value.Length;
                Text.UpdateValue($"<{Color}>{Value}");
                Text.AlphaModifier = NormalAlpha;
            }
        }
        else if (controllerManager.IsKeyPressedWithRepeat(Keys.Down))
        {
            if (HistoryIndex > 1)
            {
                var value = History[History.Count - --HistoryIndex];
                Value.Clear();
                Value.Append(value);
                CursorIndex = value.Length;
                Text.UpdateValue($"<{Color}>{Value}");
                Text.AlphaModifier = NormalAlpha;
            }
            else if (HistoryIndex == 1)
            {
                HistoryIndex = 0;
                CursorIndex = 0;
                Value.Clear();
                Text.UpdateValue($"<{Color}>{Placeholder}");
                Text.AlphaModifier = PlaceholderAlpha;
            }
        }
        else
        {
            var toUpper = controllerManager.CurrentKeyboardState.CapsLock || controllerManager.IsKeyDown(Keys.LeftShift) || controllerManager.IsKeyDown(Keys.RightShift);
            var previousValueLength = Value.Length;
            foreach (var key in controllerManager.CurrentKeyboardState.GetPressedKeys())
            {
                if (key == Keys.LeftShift || key == Keys.Right)
                {
                    continue;
                }
                if (TryConvertEnglishUSKeyboard(key, toUpper, out var c)
                    && controllerManager.IsKeyPressedWithRepeat(key))
                {
                    if (CursorIndex == Value.Length)
                    {
                        Value.Append(c);
                    }
                    else
                    {
                        Value.Insert(CursorIndex, c);
                    }
                    ++CursorIndex;
                }
            }
            if (previousValueLength != Value.Length)
            {
                Text.UpdateValue($"<{Color}>{Value}");
                Text.AlphaModifier = NormalAlpha;
                ShowCaret = true;
                CaretFlashTime = CaretFlashTimeMax;
            }
        }
        base.Update(gameTime, state, uiSpriteSheet, controllerManager);
    }

    public override void Draw(SpriteBatch spriteBatch, Rectangle drawArea, Point offset)
    {
        base.Draw(spriteBatch, drawArea, offset);
        if (ShowCaret)
        {
            var caretX = Text.GetXPosition(CursorIndex);
            spriteBatch.DrawString(Text.Normal, "_", new Vector2(x: caretX, y: Text.DestinationCache.Y + offset.Y + 1), ColorPalette.Parse(Color));
        }
    }

    private static bool TryConvertEnglishUSKeyboard(Keys key, bool toUpper, out char character)
    {
        switch (key)
        {
            case Keys.OemTilde: character = toUpper ? '~' : '`'; return true;
            case Keys.OemComma: character = toUpper ? Text.PlaceholderLT : ','; return true;
            case Keys.OemPeriod: character = toUpper ? Text.PlaceholderGT : '.'; return true;
            case Keys.OemSemicolon: character = toUpper ? ':' : ';'; return true;
            case Keys.OemQuestion: character = toUpper ? '?' : '/'; return true;
            case Keys.OemQuotes: character = toUpper ? '"' : '\''; return true;
            case Keys.OemOpenBrackets: character = toUpper ? '{' : '['; return true;
            case Keys.OemCloseBrackets: character = toUpper ? '}' : ']'; return true;
            case Keys.OemMinus: character = toUpper ? '_' : '-'; return true;
            case Keys.OemPlus: character = toUpper ? '+' : '='; return true;
            case Keys.OemPipe: character = toUpper ? '\\' : '|'; return true;
            case Keys.D0: character = toUpper ? ')' : '0'; return true;
            case Keys.D1: character = toUpper ? '!' : '1'; return true;
            case Keys.D2: character = toUpper ? '@' : '2'; return true;
            case Keys.D3: character = toUpper ? '#' : '3'; return true;
            case Keys.D4: character = toUpper ? '$' : '4'; return true;
            case Keys.D5: character = toUpper ? '%' : '5'; return true;
            case Keys.D6: character = toUpper ? '^' : '6'; return true;
            case Keys.D7: character = toUpper ? '&' : '7'; return true;
            case Keys.D8: character = toUpper ? '*' : '8'; return true;
            case Keys.D9: character = toUpper ? '(' : '9'; return true;
            case Keys.NumPad0: character = '0'; return true;
            case Keys.NumPad1: character = '1'; return true;
            case Keys.NumPad2: character = '2'; return true;
            case Keys.NumPad3: character = '3'; return true;
            case Keys.NumPad4: character = '4'; return true;
            case Keys.NumPad5: character = '5'; return true;
            case Keys.NumPad6: character = '6'; return true;
            case Keys.NumPad7: character = '7'; return true;
            case Keys.NumPad8: character = '8'; return true;
            case Keys.NumPad9: character = '9'; return true;
            case Keys.Subtract: character = '-'; return true;
            case Keys.Add: character = '+'; return true;
            case Keys.Decimal: character = '.'; return true;
            case Keys.Multiply: character = '*'; return true;
            case Keys.Divide: character = '/'; return true;
            case Keys.Space: character = ' '; return true;
            default:
                character = (char)key;
                if (char.IsLetter(character))
                {
                    if (!toUpper)
                    {
                        character = character.ToString().ToLower()[0];
                    }
                    return true;
                }
                return false;
        };
    }
}
