using Godot;
using System;

enum AutoSpriteShape { SQUARE, CAPSULE }

[Tool]
public class AutoSprite : Sprite
{
    private const float GENERATION_COOLDOWN = .5f;

    [Export]
    private AutoSpriteShape shape = AutoSpriteShape.SQUARE;

    [Export]
    private int width = 32;

    [Export]
    private int height = 0;

    [Export]
    private Color color = new Color(1f, 1f, 1f);

    private readonly ImageTexture imageTexture = new ImageTexture();
    private Image image = null;

    private string lastImageParams = null;

    private float now = 0f;
    private float lastGenerated = -GENERATION_COOLDOWN;

    public override void _Process(float delta)
    {
        if (!Engine.EditorHint)
        {
            return;
        }
        now += delta;
        // Rotation += Mathf.Pi * delta;
        if (height <= 0) height = width;
        else if (width <= 0) width = height;
        if ((Texture != null && !(Texture is ImageTexture)) || now < lastGenerated + GENERATION_COOLDOWN || lastImageParams == serializeImageParams())
        {
            return;
        }
        if (image == null)
        {
            image = new Image();
            image.Create(width, height, false, Image.Format.Rgba8);
        }
        else
        {
            if (image.GetWidth() != width || image.GetHeight() != height)
            {
                image.Resize(width, height, Image.Interpolation.Nearest);
            }
        }
        switch (shape)
        {
            case AutoSpriteShape.SQUARE: makeSquare(); break;
            case AutoSpriteShape.CAPSULE: makeCapsule(); break;
            default:
                GD.Print("Cannot handle shape " + shape.ToString());
                return;
        }
        imageTexture.CreateFromImage(image);
        imageTexture.Flags ^= (int)Texture.FlagsEnum.Filter & imageTexture.Flags;
        Texture = imageTexture;
        lastImageParams = serializeImageParams();
        GD.Print("Created image: ", lastImageParams);
        lastGenerated = now;
    }

    private void makeSquare()
    {
        image.Fill(color);
    }

    private void makeCapsule()
    {
        image.Fill(new Color(0f, 0f, 0f, 0f));
        int radius;
        if (height >= width)
        {
            radius = width / 2;
            var secondY = height - radius - 1;
            if (secondY > radius)
            {
                image.FillRect(new Rect2(0, radius, width, secondY - radius + 1), color);
            }
        }
        else
        {
            radius = height / 2;
            var secondX = width - radius - 1;
            if (secondX > radius)
            {
                image.FillRect(new Rect2(radius, 0, secondX - radius + 1, height), color);
            }
        }
        addMirroredCircles(radius);
    }

    private void addMirroredCircles(int radius)
    {
        var radiusSquared = radius * radius;
        image.Lock();
        for (int x = 0; x < radius; x++)
        {
            for (int y = 0; y <= radius; y++)
            {
                if ((x - radius) * (x - radius) + (y - radius) * (y - radius) <= radiusSquared)
                {
                    image.SetPixel(x, y, color);
                    image.SetPixel(x, height - y - 1, color);
                    image.SetPixel(width - x - 1, y, color);
                    image.SetPixel(width - x - 1, height - y - 1, color);
                }
            }
        }
        image.Unlock();
    }

    private string serializeImageParams() => String.Format("{0}x{1}: {2} ({3})", width, height, shape, color);
}
