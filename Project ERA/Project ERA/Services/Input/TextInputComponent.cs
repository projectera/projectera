using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectERA.Services.Input
{

    internal class TextInputComponent : GameComponent
    {
        private KeyboardInputComponent _keyboardInput;
        private StringBuilder _stringBuilder;

        /// <summary>
        /// Sets the key to delete previous characters
        /// </summary>
        /// <remarks>default: Backspace</remarks>
        public Keys BackKey
        {
            get; set; 
        }

        /// <summary>
        /// Sets the key to delete characters
        /// </summary>
        /// <remarks>default: Delete</remarks>
        public Keys DeleteKey
        {
            get; set;
        }

        /// <summary>
        /// NOTE make flag to disable this
        /// NOTE make flag to blink? this
        /// String that is inserted at cursor position
        /// </summary>
        /// <remarks>defaukt: Underscore</remarks>
        public String CursorChar
        {
            get;
            set;
        }

        /// <summary>
        /// If enabled, enables _ (Shift + OEM Minus)
        /// </summary>
        /// <remarks>default: true</remarks>
        public Boolean UnderscoreEnabled
        {
            get; set;
        }

        /// <summary>
        /// If enabled, enables - (OEM Minus)
        /// </summary>
        /// <remarks>default: true</remarks>
        public Boolean HyphenEnabled
        {
            get; set;
        }

        /// <summary>
        /// If enabled, enables [space]
        /// </summary>
        /// <remarks>default: true</remarks>
        public Boolean SpacesEnabled
        {
            get; set;
        }

        /// <summary>
        /// If enabled, enabled D0-D9
        /// </summary>
        /// <remarks>default: true</remarks>
        public Boolean DigitsEnabled
        {
            get; set;
        }

        /// <summary>
        /// If enabled, enables a-z
        /// </summary>
        /// <remarks>default: true</remarks>
        public Boolean LettersEnabled
        {
            get; set;
        }

        /// <summary>
        /// If enabled, enables Shift + A-Z
        /// </summary>
        /// <remarks>default: true, needs LettersEnabled</remarks>
        public Boolean UppercaseEnabled
        {
            get; set;
        }

        /// <summary>
        /// If enabled, enables Shift + D0-D9
        /// </summary>
        /// <remarks>default: true, need DigitsEnabled</remarks>
        public Boolean DigitsUppercaseEnabled
        {
            get; set;
        }

        /// <summary>
        /// Gets/Sets the maximum length of input
        /// </summary>
        /// <remarks>default: 30</remarks>
        public Int32 MaxLength
        {
            get { return _stringBuilder.Capacity; }
            set { _stringBuilder.Capacity = value; }
        }

        /// <summary>
        /// Get/Sets the cursor index
        /// </summary>
        public Int32 CursorIndex
        {
            get; set;
        }

        /// <summary>
        /// Gets/Sets the current text
        /// </summary>
        /// <remarks>Setting this property purges the data from the internal stringBuilder and appends 
        /// the value set. You might want to consider using the class functions Add and Remove.</remarks>
        public String Text
        {
            get
            {
                String text = _stringBuilder.ToString();

                if (this.Enabled && !String.IsNullOrEmpty(this.CursorChar))
                    text = text.Insert(this.CursorIndex, this.CursorChar);

                return text;
            }
            set
            {
                _stringBuilder.Clear();
                _stringBuilder.Append(value);
                this.CursorIndex = 0;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game to bind to</param>
        public TextInputComponent(Game game, Int32 maxLength = 30)
            : base(game)
        {
            _stringBuilder = new StringBuilder(maxLength);

            // Set default properties
            this.BackKey = Keys.Back;
            this.DeleteKey = Keys.Delete;
            this.SpacesEnabled = true;
            this.UnderscoreEnabled = true;
            this.HyphenEnabled = true;
            this.LettersEnabled = true;
            this.UppercaseEnabled = true;
            this.DigitsEnabled = true;
            this.DigitsUppercaseEnabled = true;
            this.CursorChar = "_";

            // Enable component
            this.Enabled = true;
        }

        /// <summary>
        /// Initialize Component
        /// </summary>
        public override void Initialize()
        {
            InputManager inputManager = (InputManager)this.Game.Services.GetService(typeof(InputManager));

            if (inputManager == null)
                throw new InvalidOperationException("No inputmanager available.");

            _keyboardInput = inputManager.Keyboard;

            // Updating after input is updated
            this.UpdateOrder = inputManager.UpdateOrder + 10;

            // Initialize Component
            base.Initialize();
        }

        /// <summary>
        /// Frame renewal
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();

            // Update component
            base.Update(gameTime);
        }

        public void HandleInput()
        {
            // Removing characters
            if (this.CursorIndex > 0)
                if (_keyboardInput.IsKeyTriggerd(BackKey))
                    if (this.CursorIndex-- < this.Text.Length + 1)
                        Remove(this.CursorIndex, 1);

            if (this.CursorIndex < _stringBuilder.Length)
                if (_keyboardInput.IsKeyTriggerd(DeleteKey))
                    Remove(this.CursorIndex, 1);

            // After this line: adding characters
            if (_stringBuilder.Length >= this.MaxLength)
                return;

            if (this.SpacesEnabled && _keyboardInput.IsKeyPressed(Keys.Space))// | _keyboardInput.IsKeyTriggerd(Keys.Space))
                Add(" ");

            Boolean upper = _keyboardInput.IsKeyDown(Keys.LeftShift) | _keyboardInput.IsKeyDown(Keys.RightShift);

            // Special Chars
            if (this.UnderscoreEnabled && upper && _keyboardInput.IsKeyPressed(Keys.OemMinus))
                Add("_");
            if (this.HyphenEnabled && !upper && _keyboardInput.IsKeyPressed(Keys.OemMinus))
                Add("-");

            // Letters
            if (this.LettersEnabled)
                foreach (Keys letter in _keyboardInput.LETTERS)
                    if (_keyboardInput.IsKeyTriggerd(letter)) //| _keyboardInput.IsKeyTriggerd(letter))
                        AddLetter(letter, this.UppercaseEnabled && upper);

            // Digits
            if (this.DigitsEnabled)
                foreach (Keys digit in _keyboardInput.DIGITS)
                    if (_keyboardInput.IsKeyPressed(digit)) //| _keyboardInput.IsKeyTriggerd(digit))
                        AddDigit(digit, this.DigitsUppercaseEnabled && upper);
        }

        /// <summary>
        /// Adds a letter to the text
        /// </summary>
        /// <param name="key">Letter as key</param>
        /// <param name="upper">Uppercase</param>
        public void AddLetter(Keys key, Boolean upper)
        {
            Add(upper ? System.Enum.GetName(typeof(Keys), key).ToUpper() : System.Enum.GetName(typeof(Keys), key).ToLower());
        }

        /// <summary>
        /// Adds a digit to the text
        /// </summary>
        /// <param name="key">Digit as key</param>
        /// <param name="upper">Uppercase</param>
        public void AddDigit(Keys key, Boolean upper)
        {
            Add(upper ? _keyboardInput.DIGITStoCAPITALSTRING[key] : _keyboardInput.DIGITStoSTRING[key]);
        }

        /// <summary>
        /// Add text and moves cursor accordingly
        /// </summary>
        /// <param name="text"></param>
        public void Add(String text)
        {
            _stringBuilder.Insert(Math.Min(this.Text.Length, this.CursorIndex), text);
            this.CursorIndex += text.Length;
        }

        /// <summary>
        /// Removes text from input
        /// </summary>
        /// <param name="index">starting index</param>
        /// <param name="length">length to remove</param>
        public void Remove(Int32 index, Int32 length)
        {
            _stringBuilder.Remove(index, length);
        }
    }
}
