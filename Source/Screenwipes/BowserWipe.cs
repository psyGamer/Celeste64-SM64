using LibSM64;

namespace Celeste64.Mod.SuperMario64;

public class BowserWipe() : ScreenWipe(FadeDuration)
{
    private const int SM64_BOWSER_TEX_X = 3 * 64;
    private const int SM64_BOWSER_TEX_Y = 16;
    private const int SM64_BOWSER_TEX_W = 32;
    private const int SM64_BOWSER_TEX_H = 64;
    
    private const float StartingSize = 17.0f;
    private const float FadeDuration = 1.6f;

    public override void Start() { }
    public override void Step(float _) { }

    public override void Render(Batcher batch, Rect bounds)
    {
        if ((Percent <= 0 && IsFromBlack) || (Percent >= 1 && !IsFromBlack))
        {
            batch.Rect(bounds, Color.Black);
            return;
        }
        
        float scale = (IsFromBlack ? Percent : (1.0f - Percent)) * StartingSize;
        
        batch.PushBlend(new BlendMode(BlendOp.Add, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha));
        batch.SetTexture(SM64Context.MenuTexture);

        // Right Half
        batch.Quad(
            bounds.Center + new Vec2(0.0f, -SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(SM64_BOWSER_TEX_W, -SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(SM64_BOWSER_TEX_W, SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(0.0f, SM64_BOWSER_TEX_H / 2.0f) * scale, 
            new Vec2(SM64_BOWSER_TEX_X / (float)SM64Context.SM64_MENU_TEXTURE_WIDTH, SM64_BOWSER_TEX_Y / (float)SM64Context.SM64_MENU_TEXTURE_HEIGHT), 
            new Vec2((SM64_BOWSER_TEX_X + SM64_BOWSER_TEX_W) / (float)SM64Context.SM64_MENU_TEXTURE_WIDTH, SM64_BOWSER_TEX_Y / (float)SM64Context.SM64_MENU_TEXTURE_HEIGHT),
            new Vec2((SM64_BOWSER_TEX_X + SM64_BOWSER_TEX_W) / (float)SM64Context.SM64_MENU_TEXTURE_WIDTH, (SM64_BOWSER_TEX_Y + SM64_BOWSER_TEX_H) / (float)SM64Context.SM64_MENU_TEXTURE_HEIGHT),
            new Vec2(SM64_BOWSER_TEX_X / (float)SM64Context.SM64_MENU_TEXTURE_WIDTH, (SM64_BOWSER_TEX_Y + SM64_BOWSER_TEX_H) / (float)SM64Context.SM64_MENU_TEXTURE_HEIGHT),
            Color.Black);
        // Left Half
        batch.Quad(
            bounds.Center + new Vec2(-SM64_BOWSER_TEX_W, -SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(0.0f, -SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(0.0f, SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(-SM64_BOWSER_TEX_W, SM64_BOWSER_TEX_H / 2.0f) * scale, 
            new Vec2((SM64_BOWSER_TEX_X + SM64_BOWSER_TEX_W) / (float)SM64Context.SM64_MENU_TEXTURE_WIDTH, SM64_BOWSER_TEX_Y / (float)SM64Context.SM64_MENU_TEXTURE_HEIGHT),
            new Vec2(SM64_BOWSER_TEX_X / (float)SM64Context.SM64_MENU_TEXTURE_WIDTH, SM64_BOWSER_TEX_Y / (float)SM64Context.SM64_MENU_TEXTURE_HEIGHT), 
            new Vec2(SM64_BOWSER_TEX_X / (float)SM64Context.SM64_MENU_TEXTURE_WIDTH, (SM64_BOWSER_TEX_Y + SM64_BOWSER_TEX_H) / (float)SM64Context.SM64_MENU_TEXTURE_HEIGHT),
            new Vec2((SM64_BOWSER_TEX_X + SM64_BOWSER_TEX_W) / (float)SM64Context.SM64_MENU_TEXTURE_WIDTH, (SM64_BOWSER_TEX_Y + SM64_BOWSER_TEX_H) / (float)SM64Context.SM64_MENU_TEXTURE_HEIGHT),
            Color.Black);
        
        const float VoidExtend = 10.0f;
        // Don't change the texture and just use the top-left part of the image (full black)
        // However move it a bit down-right, since the very top-left edge is actually transparent :catplush:
        var blackUV = new Vec2((SM64_BOWSER_TEX_X + 1) / (float)SM64Context.SM64_MENU_TEXTURE_WIDTH, (SM64_BOWSER_TEX_Y + 1) / (float)SM64Context.SM64_MENU_TEXTURE_HEIGHT);
        
        // Right Fill
        batch.Quad(
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.Top - VoidExtend),
            bounds.TopRight + Vec2.UnitX * VoidExtend - Vec2.UnitY * VoidExtend,
            bounds.BottomRight + Vec2.UnitX * VoidExtend + Vec2.UnitY * VoidExtend,
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.Bottom + VoidExtend),
            blackUV, blackUV, blackUV, blackUV,
            Color.Black);
        // Left Fill
        batch.Quad(
            bounds.TopLeft - Vec2.UnitX * VoidExtend - Vec2.UnitY * VoidExtend,
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.Top - VoidExtend),
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.Bottom + VoidExtend),
            bounds.BottomLeft - Vec2.UnitX * VoidExtend + Vec2.UnitY * VoidExtend,
            blackUV, blackUV, blackUV, blackUV,
            Color.Black);
        // Top Fill
        batch.Quad(
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.Top - VoidExtend),
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.Top - VoidExtend),
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.CenterY - SM64_BOWSER_TEX_H / 2.0f * scale),
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.CenterY - SM64_BOWSER_TEX_H / 2.0f * scale),
            blackUV, blackUV, blackUV, blackUV,
            Color.Black);
        // Bottom Fill
        batch.Quad(
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.CenterY + SM64_BOWSER_TEX_H / 2.0f * scale),
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.CenterY + SM64_BOWSER_TEX_H / 2.0f * scale),
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.Bottom + VoidExtend),
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.Bottom + VoidExtend),
            blackUV, blackUV, blackUV, blackUV,
            Color.Black);
        
        batch.PopBlend();
    }
}