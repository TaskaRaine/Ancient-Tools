using AncientTools.Entities;
using AncientTools.Utility;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AncientTools.EntityRenderers
{
    class CartRenderer : MobileStorageRenderer
    {
        /*
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
                        {
                            if (InventoryShape != null)
                            {
                                if(InventoryShape.Textures.ContainsKey(textureCode))
                                    texturePath = InventoryShape.Textures[textureCode];
                            }
                            else
                                texturePath = CartEntity.Properties.Client.FirstTexture.Base;
                        }   
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
        */
        private const float DROPPED_HEIGHT = -0.2f;

        public EntityCart CartEntity { get; set; }
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
        //public Shape InventoryShape { get; set; }

        public MeshRef MeshRef { get; set; }
        public MeshData MeshData { get; set; } = new MeshData();

        public Matrixf ModelMat = new Matrixf();

        //public Vec3f LookAtVector { get; set; } = new Vec3f(DROPPED_HEIGHT, 0.0f, 0.0f);

        //public Size2i AtlasSize => Capi.EntityTextureAtlas.Size;
        public CartRenderer(EntityCart entity, ICoreClientAPI api): base(entity, api)
        {
            CartEntity = entity;

            CartEntity.SetRendererReference(this);
        }
        public override void OnEntityLoaded()
        {
            InitializeShape();

            //-- Required to prevent a crash on game load when an inventory is already placed on a cart --//
            if (!CartEntity.MobileStorageInventory[0].Empty)
            {
                if (CartEntity.MobileStorageInventory[0].Itemstack.Collectible is BlockGenericTypedContainer)
                {
                    string type = CartEntity.MobileStorageInventory[0].Itemstack.Attributes.GetString("type");

                    AssignStoragePlacementProperties(CartEntity.MobileStorageInventory[0]?.Itemstack?.Collectible?.Attributes["cartPlacable"][type]);
                }
                else
                    AssignStoragePlacementProperties(CartEntity.MobileStorageInventory[0]?.Itemstack?.Collectible?.Attributes["cartPlacable"]);
            }
        }
        public void InitializeShape()
        {
            CurrentShape = Capi.Assets.TryGet("ancienttools:shapes/entity/cart_lg_woodwheel.json").ToObject<Shape>();
        }
        public override void BeforeRender(float dt)
        {
            TesselateShape();
        }
        public virtual void TesselateShape()
        {
            if(!CartEntity.MobileStorageInventory[0].Empty)
            {
                if (CartEntity.MobileStorageInventory[0].Itemstack.Collectible is BlockGenericTypedContainer)
                {
                    string type = CartEntity.MobileStorageInventory[0].Itemstack.Attributes.GetString("type");

                    try 
                    {
                        InventoryShapes[0] = Capi.Assets.Get<Shape>(new AssetLocation(CartEntity.MobileStorageInventory[0].Itemstack.Collectible.Attributes["mobileStorageProps"][type]["shape"].AsString() + ".json"));
                    }
                    catch
                    {
                        InventoryShapes[0] = Capi.Assets.Get<Shape>(new AssetLocation(CartEntity.MobileStorageInventory[0].Itemstack.Block.Shape.Base.Domain, "shapes/" + CartEntity.MobileStorageInventory[0].Itemstack.Block.Shape.Base.Path + ".json")) as Shape;
                    }
                }
                else
                    InventoryShapes[0] = Capi.Assets.Get<Shape>(new AssetLocation(CartEntity.MobileStorageInventory[0].Itemstack.Block.Shape.Base.Domain, "shapes/" + CartEntity.MobileStorageInventory[0].Itemstack.Block.Shape.Base.Path + ".json")) as Shape;
            }
            else if(CurrentShape.Elements[0].Children[2].Children != null)
            {
                CurrentShape.Elements[0].Children[2].Children = null;
                InventoryShapes[0] = null;
            }

            MeshData = GenMesh();

            UpdateMesh(MeshData);
        }
        public MeshData GenMesh()
        {
            CurrentShape.Elements[0].RotationY = TaskaMath.ShortLerpDegrees(CurrentShape.Elements[0].RotationY, CartEntity.LookAtVector.Y * GameMath.RAD2DEG, 0.05);
            CartElement.RotationX = TaskaMath.ShortLerpDegrees(CartElement.RotationX, CartEntity.LookAtVector.X * GameMath.RAD2DEG, 0.05);
            
            if(InventoryShapes[0] != null && StoragePlacementProperties.IsSet())
            {
                CurrentShape.Elements[0].Children[2] = InventoryShapes[0].Elements[0];
                CurrentShape.Elements[0].Children[2].From = StoragePlacementProperties.Translation;
                CurrentShape.Elements[0].Children[2].To = StoragePlacementProperties.Translation;
                CurrentShape.Elements[0].Children[2].RotationOrigin = CartElement.RotationOrigin;
                CurrentShape.Elements[0].Children[2].RotationX = CartElement.RotationX + StoragePlacementProperties.Rotation[0];
                CurrentShape.Elements[0].Children[2].RotationY = StoragePlacementProperties.Rotation[1];
                CurrentShape.Elements[0].Children[2].RotationZ = StoragePlacementProperties.Rotation[2];
                CurrentShape.Elements[0].Children[2].ScaleX = StoragePlacementProperties.Scale;
                CurrentShape.Elements[0].Children[2].ScaleY = StoragePlacementProperties.Scale;
                CurrentShape.Elements[0].Children[2].ScaleZ = StoragePlacementProperties.Scale;
            }

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
        public override void Dispose()
        {
            MeshRef?.Dispose();
        }
    }
}
