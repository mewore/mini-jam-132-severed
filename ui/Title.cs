using Godot;

enum TitleType { ALL_ON_SEPARATE_LINES, ALL_ON_ONE_LINE, UPPER, LOWER }

[Tool]
public class Title : Label
{
    private const string SEPARATOR = ":";

    private TitleType _type = TitleType.ALL_ON_SEPARATE_LINES;

    [Export]
    private TitleType type
    {
        get => _type; set
        {
            _type = value;
            Text = getTitleText();
        }
    }

    public override void _Ready()
    {
        Text = getTitleText();
    }

    private string getTitleText()
    {
        string projectName = ProjectSettings.GetSetting("application/config/name").ToString();
        if (_type == TitleType.ALL_ON_ONE_LINE || !projectName.Contains(SEPARATOR))
        {
            return projectName;
        }
        if (_type == TitleType.ALL_ON_SEPARATE_LINES)
        {
            return projectName.Replace(SEPARATOR, "\n");
        }
        if (_type == TitleType.UPPER)
        {
            return projectName.Substring(0, projectName.IndexOf(SEPARATOR));
        }
        return projectName.Substring(projectName.IndexOf(SEPARATOR) + 1).Trim();
    }
}
