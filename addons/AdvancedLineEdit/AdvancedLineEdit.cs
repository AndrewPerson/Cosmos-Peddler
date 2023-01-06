using System.Threading.Tasks;

namespace Godot;

[Tool]
public partial class AdvancedLineEdit : LineEdit
{
    public delegate string? ValidationDelegate(string text);
    public delegate Task<string?> AsyncValidationDelegate(string text);
    
    public enum State
    {
        None,
        Loading,
        Valid,
        Invalid
    }

    private readonly static Image loadingIcon = GD.Load<Image>("res://addons/AdvancedLineEdit/loading.png");
    private readonly static Image validIcon = GD.Load<Image>("res://addons/AdvancedLineEdit/valid.png");
    private readonly static Image invalidIcon = GD.Load<Image>("res://addons/AdvancedLineEdit/invalid.png");

    private State _validationState = State.None;

    [Export(PropertyHint.Enum, "None,Loading,Valid,Invalid")]
    public State ValidationState
    {
        get => _validationState;
        set
        {
            _validationState = value;
            SetState(value);
        }
    }

    private string _label = "";

    [Export]
    public string Label
    {
        get => _label;
        set
        {
            _label = value;
            QueueRedraw();
        }
    }

    private string _validationMessage = "";

    [Export]
    public string ValidationMessage
    {
        get => _validationMessage;
        set
        {
            _validationMessage = value;
            QueueRedraw();
        }
    }

    private ImageTexture? loadingIconTexture;
    private ImageTexture? validIconTexture;
    private ImageTexture? invalidIconTexture;

    public bool Validate(ValidationDelegate validator)
    {
        var message = validator(Text);

        if (message == null)
        {
            ValidationState = State.Valid;
            return true;
        }
        else
        {
            ValidationState = State.Invalid;
            ValidationMessage = message;
            return false;
        }
    }

    public async Task<bool> Validate(AsyncValidationDelegate validator)
    {
        ValidationState = State.Loading;

        var message = await validator(Text);

        if (message == null)
        {
            ValidationState = State.Valid;
            return true;
        }
        else
        {
            ValidationState = State.Invalid;
            ValidationMessage = message;
            return false;
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.AsText().Length > 0)
        {
            ValidationState = State.None;
        }
    }

    public override void _Draw()
    { 
        base._Draw();

        DrawString(ThemeDB.FallbackFont, new Vector2(0, -ThemeDB.FallbackFontSize / 3), _label, fontSize: ThemeDB.FallbackFontSize + 2);
        DrawStringOutline(ThemeDB.FallbackFont, new Vector2(0, -ThemeDB.FallbackFontSize / 3), _label, fontSize: ThemeDB.FallbackFontSize + 2, size: 2, modulate: new Color(0, 0, 0));

        if (ValidationState == State.Invalid) DrawString(ThemeDB.FallbackFont, new Vector2(0, Size.y + ThemeDB.FallbackFontSize), _validationMessage, fontSize: ThemeDB.FallbackFontSize, modulate: new Color(1, 0, 0));
    }

    private void SetState(State state)
    {
        var height = Mathf.FloorToInt(Size.y) - 20;

        switch (state)
        {
            case State.None:
                RightIcon = null;
                break;
            case State.Loading:
                if (loadingIconTexture == null || loadingIconTexture.GetSize() != new Vector2(height, height))
                {
                    var resizedLoadingIcon = (Image)loadingIcon.Duplicate();
                    resizedLoadingIcon.Resize(height, height);

                    loadingIconTexture = ImageTexture.CreateFromImage(resizedLoadingIcon);
                }

                RightIcon = loadingIconTexture;

                break;
            case State.Valid:
                if (validIconTexture == null || validIconTexture.GetSize() != new Vector2(height, height))
                {
                    var resizedValidIcon = (Image)validIcon.Duplicate();
                    resizedValidIcon.Resize(height, height);

                    validIconTexture = ImageTexture.CreateFromImage(resizedValidIcon);
                }

                RightIcon = validIconTexture;

                break;
            case State.Invalid:
                if (invalidIconTexture == null || invalidIconTexture.GetSize() != new Vector2(height, height))
                {
                    var resizedInvalidIcon = (Image)invalidIcon.Duplicate();
                    resizedInvalidIcon.Resize(height, height);

                    invalidIconTexture = ImageTexture.CreateFromImage(resizedInvalidIcon);
                }

                RightIcon = invalidIconTexture;

                break;
        }
    }
}