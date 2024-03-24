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
        batch.PushSampler(new TextureSampler(TextureFilter.Linear, TextureWrap.ClampToEdge, TextureWrap.ClampToEdge));
        batch.SetTexture(SM64Context.MenuTexture);

        // Need to apply slight offset, since otherwise the transparency on the edge would bleed into it
        const float Eps = 1f;
        var uvTL = new Vec2((SM64_BOWSER_TEX_X + Eps)                     / SM64Context.SM64_MENU_TEXTURE_WIDTH, (SM64_BOWSER_TEX_Y + Eps)                     / SM64Context.SM64_MENU_TEXTURE_HEIGHT);
        var uvTR = new Vec2((SM64_BOWSER_TEX_X + SM64_BOWSER_TEX_W - Eps) / SM64Context.SM64_MENU_TEXTURE_WIDTH, (SM64_BOWSER_TEX_Y + Eps)                     / SM64Context.SM64_MENU_TEXTURE_HEIGHT);
        var uvBL = new Vec2((SM64_BOWSER_TEX_X + Eps)                     / SM64Context.SM64_MENU_TEXTURE_WIDTH, (SM64_BOWSER_TEX_Y + SM64_BOWSER_TEX_H - Eps) / SM64Context.SM64_MENU_TEXTURE_HEIGHT);
        var uvBR = new Vec2((SM64_BOWSER_TEX_X + SM64_BOWSER_TEX_W - Eps) / SM64Context.SM64_MENU_TEXTURE_WIDTH, (SM64_BOWSER_TEX_Y + SM64_BOWSER_TEX_H - Eps) / SM64Context.SM64_MENU_TEXTURE_HEIGHT);
        
        // Right Half
        batch.Quad(
            bounds.Center + new Vec2(0.0f, -SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(SM64_BOWSER_TEX_W, -SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(SM64_BOWSER_TEX_W, SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(0.0f, SM64_BOWSER_TEX_H / 2.0f) * scale, 
            uvTL, uvTR, uvBR, uvBL,
            Color.Black);
        // Left Half
        batch.Quad(
            bounds.Center + new Vec2(-SM64_BOWSER_TEX_W, -SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(0.0f, -SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(0.0f, SM64_BOWSER_TEX_H / 2.0f) * scale, 
            bounds.Center + new Vec2(-SM64_BOWSER_TEX_W, SM64_BOWSER_TEX_H / 2.0f) * scale, 
            uvTR, uvTL, uvBL, uvBR,
            Color.Black);
        
        const float VoidExtend = 10.0f;
        
        // Right Fill
        batch.Quad(
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.Top - VoidExtend),
            bounds.TopRight + Vec2.UnitX * VoidExtend - Vec2.UnitY * VoidExtend,
            bounds.BottomRight + Vec2.UnitX * VoidExtend + Vec2.UnitY * VoidExtend,
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.Bottom + VoidExtend),
            uvTL, uvTL, uvTL, uvTL,
            Color.Black);
        // Left Fill
        batch.Quad(
            bounds.TopLeft - Vec2.UnitX * VoidExtend - Vec2.UnitY * VoidExtend,
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.Top - VoidExtend),
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.Bottom + VoidExtend),
            bounds.BottomLeft - Vec2.UnitX * VoidExtend + Vec2.UnitY * VoidExtend,
            uvTL, uvTL, uvTL, uvTL,
            Color.Black);
        // Top Fill
        batch.Quad(
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.Top - VoidExtend),
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.Top - VoidExtend),
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.CenterY - SM64_BOWSER_TEX_H / 2.0f * scale),
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.CenterY - SM64_BOWSER_TEX_H / 2.0f * scale),
            uvTL, uvTL, uvTL, uvTL,
            Color.Black);
        // Bottom Fill
        batch.Quad(
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.CenterY + SM64_BOWSER_TEX_H / 2.0f * scale),
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.CenterY + SM64_BOWSER_TEX_H / 2.0f * scale),
            new Vec2(bounds.CenterX + SM64_BOWSER_TEX_W * scale, bounds.Bottom + VoidExtend),
            new Vec2(bounds.CenterX - SM64_BOWSER_TEX_W * scale, bounds.Bottom + VoidExtend),
            uvTL, uvTL, uvTL, uvTL,
            Color.Black);
        
        batch.PopSampler();
        batch.PopBlend();
    }
}