using AncientTools.Entities;
using AncientTools.Utility;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace AncientTools.EntityRenderers
{
    class CartRenderer : EntityRenderer, ITexPositionSource
    {
        public ICoreClientAPI Capi { get; set; }
        public EntityCart CartEntity { get; set; }
        public Shape CurrentShape { get; set; }
        public ShapeElement CartElement { 
            get
            {
                if (CurrentShape != null)
                    return CurrentShape.Elements[0].Children[0];

                return null;
            }
            set
            {
                CurrentShape.Elements[0].Children[0] = value;
            }
        }
        public ShapeElement AxleElement { 
            get 
            {
                if (CurrentShape != null)
                    return CurrentShape.Elements[0].Children[1];

                return null;
            }
            private set
            {
                CurrentShape.Elements[0].Children[1] = value;
            }
        }

        public MeshRef MeshRef { get; set; }
        public MeshData MeshData { get; set; } = new MeshData();

        public Matrixf ModelMat = new Matrixf();

        public Vec3f LookAtVector { get; set; } = new Vec3f(0.0f, 0.0f, 0.0f);
        public Size2i AtlasSize => Capi.EntityTextureAtlas.Size;

        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                AssetLocation texturePath = null;

                if (CurrentShape != null)
                {
                    if (CurrentShape.Textures == null || !CurrentShape.Textures.ContainsKey(textureCode))
                    {
                        if (CartEntity.Properties.Client.Textures.ContainsKey(textureCode))
                            texturePath = CartEntity.Properties.Client.Textures[textureCode].Base;
                        else
                            texturePath = CartEntity.Properties.Client.FirstTexture.Base;
                    }
                    else
                        CurrentShape.Textures.TryGetValue(textureCode, out texturePath);
                }

                TextureAtlasPosition texpos = null;

                if (texturePath != null)
                    texpos = capi.EntityTextureAtlas[texturePath];

                if (texpos == null)
                {
                    IAsset texAsset = Capi.Assets.TryGet(texturePath.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"));
                    if (texAsset != null)
                    {
                        Capi.EntityTextureAtlas.GetOrInsertTexture(texturePath, out _, out texpos);
                    }
                }

                return texpos;
            }
        }
        public CartRenderer(EntityCart entity, ICoreClientAPI api): base(entity, api)
        {
            Capi = api;
            CartEntity = entity;

            CartEntity.SetRendererReference(this);
        }
        public override void OnEntityLoaded()
        {
            InitializeShape();
        }
        public void InitializeShape()
        {
            CurrentShape = Capi.Assets.TryGet("ancienttools:shapes/entity/cart_lg.json").ToObject<Shape>();
        }
        public override void BeforeRender(float dt)
        {
            TesselateShape();
        }
        public virtual void TesselateShape()
        {
            MeshData = GenMesh();

            UpdateMesh(MeshData);
        }
        public MeshData GenMesh()
        {
            CurrentShape.Elements[0].RotationY = TaskaMath.ShortLerpDegrees(CurrentShape.Elements[0].RotationY, LookAtVector.Y * GameMath.RAD2DEG, 0.05);
            CartElement.RotationX = TaskaMath.ShortLerpDegrees(CartElement.RotationX, LookAtVector.X * GameMath.RAD2DEG, 0.05);

            if (CartEntity.AttachedEntity != null)
                if (CartEntity.AttachedEntity.Controls.TriesToMove)
                    AxleElement.RotationX = TaskaMath.ShortLerpDegrees(AxleElement.RotationX, AxleElement.RotationX - 90.0f, 0.05);

            Capi.Tesselator.TesselateShape("der", CurrentShape, out MeshData wholeMesh, this);

            return wholeMesh;
        }
        public void UpdateMesh(MeshData mesh)
        {
            if (mesh != null)
            {
                if (MeshRef == null)
                    MeshRef = Capi.Render.UploadMesh(mesh);
                else
                {
                    //-- I should be able to use api.Render.UpdateMesh instead of uploading a brand new mesh object but this seems to cause a crash in VS 1.17 --//
                    Capi.Render.DeleteMesh(MeshRef);
                    MeshRef = null;

                    MeshRef = Capi.Render.UploadMesh(mesh);
                }
            }
        }
        public override void DoRender3DOpaque(float dt, bool isShadowPass)
        {
            if (isShadowPass) return;

            if (MeshRef != null)
            {
                IRenderAPI rapi = capi.Render;

                rapi.GlDisableCullFace();

                rapi.GlToggleBlend(true, EnumBlendMode.Standard);

                IStandardShaderProgram prog = rapi.PreparedStandardShader((int)entity.Pos.X, (int)(entity.Pos.Y + 0.2), (int)entity.Pos.Z);
                Vec3d camPos = capi.World.Player.Entity.CameraPos;
                prog.Tex2D = Capi.EntityTextureAtlas.AtlasTextureIds[0];
                prog.ModelMatrix = ModelMat
                    .Identity()
                    .Translate(entity.Pos.X - camPos.X, entity.Pos.Y - camPos.Y, entity.Pos.Z - camPos.Z)
                    //.RotateY(LookAtVector.Y)
                    //.RotateX(LookAtVector.X)
                    //.RotateZ(LookAtVector.Z)
                    .Translate(-0.5f, 0, -0.5f)
                    .Values
                ;
                prog.ViewMatrix = rapi.CameraMatrixOriginf;
                prog.ProjectionMatrix = rapi.CurrentProjectionMatrix;
                rapi.RenderMesh(MeshRef);
                prog.Stop();
            }
                //Capi.Render.RenderMesh(MeshRef);
        }
        public void SetLookAtVector(Vec3f cartPos, Vec3f attachedEntityPos, Vec3f attachedEntityEyePos)
        {
            Vec3f normal = ((attachedEntityPos + (attachedEntityEyePos / 4)) - cartPos).Normalize();
            //normal.Y = 0.325f;

            double pitch = Math.Asin(normal.Y);
            double yaw = Math.Atan2(-normal.X, -normal.Z);

            LookAtVector.X = (float)pitch;
            LookAtVector.Y = (float)yaw;
        }
        public override void Dispose()
        {
            
        }
    }
}
