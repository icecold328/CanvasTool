#if FLAX_EDITOR

using FlaxEditor.CustomEditors.Elements;
using FlaxEditor.CustomEditors;
using FlaxEditor.GUI.Input;
using FlaxEditor.GUI;
using FlaxEditor;
using FlaxEngine.GUI;
using FlaxEngine;
using System.Collections.Generic;

namespace CanvasTool;

/// <summary>
/// Canvas Tools Window.
/// </summary>
public class CanvasToolsWindow : CustomEditorWindow
{
    UICanvas[] AvailableCanvas { get; set; }
    //UI Controls From Selected Canvas.
    private UIControl[] SelectedCanvascontrols = null;

    //Information Display.
    private int fontsUpdatedCounter = 0;
    private int buttonsUpdatedCounter = 0;
    private int SlidersChangedCounter = 0;
    private int DropDownsChangedCounter = 0;
    private int DropPanelsChangedCounter = 0;
    private int CheckBoxChangedCounter = 0;

    //Default Colors
    private Color BaseColor = new Color(0.274f, 0.274f, 0.274f, 1f);
    private Color HighLightedColor = new Color(.39f, .39f, .39f, 1f);
    private Color SelectedColor = new Color(0, .47f, .8f, 1f);

    //Canvas Selection
    private bool ChangingAllCanvas;

    //Font Page.
    private bool FontChangeSelected = false;
    private bool FontAssetChanged = false;
    private Asset NewFontAsset;
    private bool FontSizeChanged = false;
    private byte NewFontSizeType = 0;
    private int NewFontSize = 0;
    private bool FontColorChanged = false;
    private Color NewFontColor = Color.White;

    //Button Page;
    private bool ButtonChangeSelected = false;
    private bool ButtonColorChanged = false;
    private Color ButtonBaseColor;
    private Color ButtonHightlightedColor;
    private Color ButtonSelectedColor;

    private bool ButtonImageChanged = false;
    private byte ImageType = 0; // 0 Texture, 1 Sprite, 2 Solid Color.
    private Asset ButtonImageAsset = null; //Either Texture or Sprite Atlas.
    private int SelectedAtlasID = -1; //always -1 unless its an atles.
    private Color ButtonSolidColor = new Color(0, 0, 0, 0);

    //Slider Page;
    private bool SliderChangeSelected = false;
    private bool TrackColorChanged = false;
    private Color TrackLineColor = Color.White;
    private Color TrackFillColor = Color.White;
    private bool ThumbColorChanged = false;
    private Color ThumBColor = Color.White;
    private Color ThumbHighColor = Color.Khaki;
    private Color ThumbSelectColor = Color.White;

    //DropDown Page.
    bool DropDownChangeSelected = false;
    bool DDBackgroundChanged = false;
    private Color DropDownBackGroundColor = Color.White;
    private Color DropDownBackGroundHighColor = Color.Khaki;
    private Color DropDownBackGroundSelectColor = Color.White;
    bool DDBoarderChanged = false;
    private Color DropDownBoarderColor = Color.White;
    private Color DropDownBoarderHighColor = Color.Khaki;
    private Color DropDownBoarderSelectColor = Color.White;
    bool DDArrowChanged = false;
    private Color DropDownArrowColor = Color.White;
    private Color DropDownArrowHighColor = Color.Khaki;
    private Color DropDownArrowSelectColor = Color.White;

    //DropPanel Page.
    private bool DropPanelChangeSelected = false;
    private bool DropPanelColorsChanged = false;
    private Color DP_H_TextColor;
    private Color DP_H_Color;
    private Color DP_H_MouseColor;
    private bool DropPanelTextureChanged = false;
    private byte DP_TextureType = 0; // 0 Texture, 1 Sprite, 2 Solid Color.
    private Asset DP_UpArrow_ImageAsset = null; //Either Texture or Sprite Atlas.
    private int DP_UpArrow_SelectedAtlasID = -1; //always -1 unless its an atles.
    private Color DP_UpArrow_SolidColor = new Color(0, 0, 0, 0);
    private Asset DP_DownArrow_ImageAsset = null; //Either Texture or Sprite Atlas.
    private int DP_DownArrow_SelectedAtlasID = -1; //always -1 unless its an atles.
    private Color DP_DownArrow_SolidColor = new Color(0, 0, 0, 0);

    //CheckBox Page.
    bool CheckBoxChangeSelected = false;
    bool CheckBoxBoardChanged = false;
    bool CheckBoxArrowChanged = false;

    Color CheckBoxBaseColor;
    Color CheckBoxHighColor;

    byte CB_TextureType = 0; // 0 Texture, 1 Sprite, 2 Solid Color.
    Asset CB_ImageAsset = null; //Either Texture or Sprite Atlas.
    int CB_SelectedAtlasID = -1; //always -1 unless its an atles.
    Color CB_SolidColor = new Color(0, 0, 0, 0);

    LabelElement errorLobel, DisplayInfo;

    void SetInitialColors()
    {
        NewFontColor = Color.White;
        ButtonBaseColor = BaseColor;
        ButtonHightlightedColor = HighLightedColor;
        ButtonSelectedColor = SelectedColor;
        ButtonSolidColor = BaseColor;
        TrackLineColor = BaseColor;
        TrackFillColor = SelectedColor;
        ThumBColor = BaseColor;
        ThumbHighColor = HighLightedColor;
        ThumbSelectColor = SelectedColor;
        DropDownBackGroundColor = BaseColor;
        DropDownBackGroundHighColor = HighLightedColor;
        DropDownBackGroundSelectColor = SelectedColor;
        DropDownBoarderColor = BaseColor;
        DropDownBoarderHighColor = HighLightedColor;
        DropDownBoarderSelectColor = SelectedColor;
        DropDownArrowColor = BaseColor;
        DropDownArrowHighColor = HighLightedColor;
        DropDownArrowSelectColor = SelectedColor;
        CheckBoxBaseColor = BaseColor;
        CheckBoxHighColor = HighLightedColor;
    }
    /// <inheritdoc />
    public override void Initialize(LayoutElementsContainer layout)
    {
        if (Window is null)
            return;

        GetAssets();

        SetInitialColors();

        #region Top Panel UI Setup Canvas Select

        errorLobel = layout.Label("");
        errorLobel.Control.Height = 1;
        errorLobel.Control.Width = 250;

        var UseAll = layout.Checkbox("Select All Active Scene Canvas's");
        var SelectionBox = layout.ComboBox("Select Active Canvas");

        PopulateCanvasBox(SelectionBox);

        layout.Space(10);
        #endregion

        #region Font UI Setup

        var CB = layout.Checkbox("Fonts");
        var FontVerticlePanel = layout.VerticalPanel();
        CB.CheckBox.StateChanged += (CheckBox obj) =>
        {
            FontVerticlePanel.Panel.Visible = obj.Checked;
            FontVerticlePanel.Panel.Enabled = obj.Checked;
            FontChangeSelected = obj.Checked;
        };

        var UseNewFontOverride = FontVerticlePanel.Checkbox(" > Change Font");
        UseNewFontOverride.CheckBox.Checked = false;
        var ChangeFontSelection = FontVerticlePanel.VerticalPanel();
        UseNewFontOverride.CheckBox.StateChanged += (CheckBox obj) =>
        {
            ChangeFontSelection.Panel.Visible = obj.Checked;
            ChangeFontSelection.Panel.Enabled = obj.Checked;
            FontAssetChanged = obj.Checked;
        };

        var FontPanel = FontVerticlePanel.VerticalPanel();
        var NewFontPicker = ChangeFontSelection.Custom<FlaxEditor.GUI.AssetPicker>("   - Select New Font :");
        var FontSelection = (AssetPicker)NewFontPicker.CustomControl;
        FontSelection.Validator.AssetType = new FlaxEditor.Scripting.ScriptType(typeof(FontAsset));
        FontSelection.SelectedItemChanged += () =>
        {
            FontAssetChanged = true;
            NewFontAsset = FontSelection.Validator.SelectedAsset;
        };
        ChangeFontSelection.Panel.Visible = false;

        var ChangeFontSizeOverride = FontVerticlePanel.Checkbox(" > Change Size");
        var FontSizePanel = FontVerticlePanel.VerticalPanel();
        ChangeFontSizeOverride.CheckBox.StateChanged += (CheckBox obj) => { FontSizePanel.Panel.Enabled = obj.Checked; FontSizePanel.Panel.Visible = obj.Checked; FontSizeChanged = obj.Checked; };
        var FontSizeChangeType = FontSizePanel.ComboBox("   - Change Type", "Additions (Original Font Size +/- Addition) / Override Replaces Original with Value");
        FontSizeChangeType.ComboBox.AddItem("Addition");
        FontSizeChangeType.ComboBox.AddItem("Override");
        FontSizeChangeType.ComboBox.SelectedIndex = 0;
        FontSizeChangeType.ComboBox.SelectedIndexChanged += (ComboBox obj) =>
        {
            NewFontSizeType = (byte)obj.SelectedIndex;
        };

        var fontSizeInt = FontSizePanel.IntegerValue("   - Size ");
        FontSizeChangeType.ComboBox.SelectedIndexChanged += (ComboBox obj) => { fontSizeInt.IntValue.Value = 0; };
        FontSizePanel.Panel.Visible = false;
        fontSizeInt.IntValue.ValueChanged += () => { NewFontSize = fontSizeInt.IntValue.Value; };

        var UseColorOverride = FontVerticlePanel.Checkbox(" > Change Color", "Replace Font Color with this Value");
        var ChangeFontColor = FontVerticlePanel.VerticalPanel();
        UseColorOverride.CheckBox.Checked = false;

        var FontColorPicker = ChangeFontColor.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Font Color");
        UseColorOverride.CheckBox.StateChanged += (CheckBox obj) =>
        {
            ChangeFontColor.Panel.Visible = obj.Checked;
            FontColorPicker.CustomControl.Enabled = obj.Checked;
            FontColorPicker.CustomControl.Visible = obj.Checked;
            FontColorPicker.CustomControl.Value = obj.Checked ? Color.White : Color.Black;
            FontColorChanged = obj.Checked;
        };
        FontColorPicker.CustomControl.ValueChanged += () => { NewFontColor = FontColorPicker.CustomControl.Value; };

        FontColorPicker.CustomControl.Visible = false;
        FontVerticlePanel.Panel.Visible = false;
        ChangeFontColor.Panel.Visible = false;
        #endregion

        #region Button UI Setup

        layout.Space(10);

        var UpdateButtons = layout.Checkbox("Buttons");
        var ButtonVerticlePanel = layout.VerticalPanel();
        UpdateButtons.CheckBox.StateChanged += (CheckBox obj) =>
        {
            ButtonVerticlePanel.Panel.Visible = obj.Checked;
            ButtonVerticlePanel.Panel.Enabled = obj.Checked;
            ButtonChangeSelected = obj.Checked;
        };

        var ButtonColorOverride = ButtonVerticlePanel.Checkbox(" > Change Colors");
        var ButtonPanel = ButtonVerticlePanel.VerticalPanel();

        ButtonColorOverride.CheckBox.StateChanged += (CheckBox obj) =>
        {
            ButtonPanel.Panel.Visible = obj.Checked;
            ButtonPanel.Panel.Enabled = obj.Checked;
            ButtonColorChanged = obj.Checked;
        };

        var BBase = ButtonPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Base Color");
        BBase.CustomControl.Value = BaseColor;
        BBase.CustomControl.ColorValueChanged += (ColorValueBox obj) =>
        {
            ButtonBaseColor = obj.Value;
        };

        var BHigh = ButtonPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Highlighted Color");
        BHigh.CustomControl.Value = HighLightedColor;
        BHigh.CustomControl.ColorValueChanged += (ColorValueBox obj) =>
        {
            ButtonHightlightedColor = obj.Value;
        };

        var BSel = ButtonPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Selected Color");
        BSel.CustomControl.Value = SelectedColor;
        BSel.CustomControl.ColorValueChanged += (ColorValueBox obj) =>
        {
            ButtonSelectedColor = obj.Value;
        };

        ButtonPanel.Panel.Visible = false;
        ButtonPanel.Panel.Enabled = false;

        var ButtonImageOverride = ButtonVerticlePanel.Checkbox(" > Change Textures/Sprites");
        var ButtonImagePanel = ButtonVerticlePanel.VerticalPanel();

        ButtonImageOverride.CheckBox.StateChanged += (CheckBox obj) =>
        {
            ButtonImagePanel.Panel.Visible = obj.Checked;
            ButtonImagePanel.Panel.Enabled = obj.Checked;
            ButtonImageChanged = obj.Checked;
        };

        var ButtonImageType = ButtonImagePanel.ComboBox("   - Texture Type");
        ButtonImageType.ComboBox.AddItem("Texture");
        ButtonImageType.ComboBox.AddItem("Sprite");
        ButtonImageType.ComboBox.AddItem("Color");


        //Texture Panel;
        var bn_vp1 = ButtonImagePanel.VerticalPanel();

        var TextureButtonImage = bn_vp1.Custom<FlaxEditor.GUI.AssetPicker>("   - Button Image");
        TextureButtonImage.CustomControl.Validator.AssetType = new FlaxEditor.Scripting.ScriptType(typeof(Texture));
        TextureButtonImage.CustomControl.SelectedItemChanged += () =>
        {
            ButtonImageAsset = TextureButtonImage.CustomControl.Validator.SelectedAsset;
        };

        bn_vp1.Panel.Visible = false;
        bn_vp1.Panel.Enabled = false;

        var bn_vp2 = ButtonImagePanel.VerticalPanel();

        var TextureButtonSprite = bn_vp2.Custom<FlaxEditor.GUI.AssetPicker>("   - Button Sprite");
        TextureButtonSprite.CustomControl.Validator.AssetType = new FlaxEditor.Scripting.ScriptType(typeof(SpriteAtlas));

        var SelectedButtonSprite = bn_vp2.ComboBox("Select Sprite");
        SelectedButtonSprite.ComboBox.SelectedIndexChanged += (ComboBox obj) => { SelectedAtlasID = obj.SelectedIndex; };

        TextureButtonSprite.CustomControl.SelectedItemChanged += () =>
        {
            var SpriteSheet = (SpriteAtlas)TextureButtonSprite.CustomControl.Validator.SelectedAsset;
            ButtonImageAsset = TextureButtonSprite.CustomControl.Validator.SelectedAsset;
            if (SpriteSheet is null)
                return;

            SelectedButtonSprite.ComboBox.ClearItems();

            for (int i = 0; i < SpriteSheet.Sprites.Length; i++)
            {
                SelectedButtonSprite.ComboBox.AddItem(SpriteSheet.Sprites[i].Name);
            }

            SelectedButtonSprite.ComboBox.SelectedIndex = 0;
        };

        bn_vp2.Panel.Visible = false;
        bn_vp2.Panel.Enabled = false;
        ButtonImagePanel.Panel.Visible = false;
        ButtonImagePanel.Panel.Enabled = false;

        var bn_vp3 = ButtonImagePanel.VerticalPanel();

        var buttoncolor = bn_vp3.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Solid Color");
        buttoncolor.CustomControl.Value = Color.White;
        buttoncolor.CustomControl.ColorValueChanged += (ColorValueBox obj) => { ButtonSolidColor = obj.Value; };

        bn_vp3.Panel.Visible = false;
        bn_vp3.Panel.Enabled = false;

        ButtonImageType.ComboBox.SelectedIndexChanged += (ComboBox obj) =>
        {
            ImageType = (byte)obj.SelectedIndex;

            bn_vp1.Panel.Visible = false;
            bn_vp1.Panel.Enabled = false;
            bn_vp2.Panel.Visible = false;
            bn_vp2.Panel.Enabled = false;
            bn_vp3.Panel.Visible = false;
            bn_vp3.Panel.Enabled = false;


            if (obj.SelectedIndex == 0)
            {
                bn_vp1.Panel.Visible = true;
                bn_vp1.Panel.Enabled = true;


            }
            if (obj.SelectedIndex == 1)
            {

                bn_vp2.Panel.Visible = true;
                bn_vp2.Panel.Enabled = true;
            }
            if (obj.SelectedIndex == 2)
            {
                bn_vp3.Panel.Visible = true;
                bn_vp3.Panel.Enabled = true;
            }
        };

        ButtonImagePanel.Panel.Visible = false;
        ButtonImagePanel.Panel.Enabled = false;
        ButtonVerticlePanel.Panel.Visible = false;
        ButtonVerticlePanel.Panel.Enabled = false;

        #endregion

        #region Slider UI Setup

        layout.Space(10);

        var US = layout.Checkbox("Sliders");
        var SliderVerticlePanel = layout.VerticalPanel();
        US.CheckBox.StateChanged += (CheckBox obj) => { SliderVerticlePanel.Panel.Visible = obj.Checked; SliderVerticlePanel.Panel.Enabled = obj.Checked; SliderChangeSelected = obj.Checked; };

        var TrackColorOverride = SliderVerticlePanel.Checkbox(" > Change Track Colors");
        var TrackColorPanel = SliderVerticlePanel.VerticalPanel();
        TrackColorOverride.CheckBox.StateChanged += (CheckBox obj) => { TrackColorPanel.Panel.Visible = obj.Checked; TrackColorPanel.Panel.Enabled = obj.Checked; TrackColorChanged = obj.Checked; };

        var TLC = TrackColorPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Line Color");
        TLC.CustomControl.Value = BaseColor;
        TLC.CustomControl.ColorValueChanged += (ColorValueBox obj) => { TrackLineColor = obj.Value; };

        var FLC = TrackColorPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Fill Line Color");
        FLC.CustomControl.Value = SelectedColor;
        FLC.CustomControl.ColorValueChanged += (ColorValueBox obj) => { TrackFillColor = obj.Value; };

        var ThumbColorOverride = SliderVerticlePanel.Checkbox(" > Change Thumb Colors");
        var ThumbColorPanel = SliderVerticlePanel.VerticalPanel();
        ThumbColorOverride.CheckBox.StateChanged += (CheckBox obj) => { ThumbColorPanel.Panel.Visible = obj.Checked; ThumbColorPanel.Panel.Enabled = obj.Checked; ThumbColorChanged = obj.Checked; };

        var TC = ThumbColorPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Color");
        TC.CustomControl.Value = BaseColor;
        TC.CustomControl.ColorValueChanged += (ColorValueBox obj) => { ThumBColor = obj.Value; };
        var THC = ThumbColorPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Hightlighted Color");
        THC.CustomControl.Value = HighLightedColor;
        THC.CustomControl.ColorValueChanged += (ColorValueBox obj) => { ThumbHighColor = obj.Value; };
        var TSC = ThumbColorPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Selected Color");
        TSC.CustomControl.Value = SelectedColor;
        TSC.CustomControl.ColorValueChanged += (ColorValueBox obj) => { ThumbSelectColor = obj.Value; };

        SliderVerticlePanel.Panel.Visible = false;
        SliderVerticlePanel.Panel.Enabled = false;
        ThumbColorPanel.Panel.Visible = false;
        ThumbColorPanel.Panel.Enabled = false;
        TrackColorPanel.Panel.Visible = false;
        TrackColorPanel.Panel.Enabled = false;

        #endregion

        #region DropBox UI Setup

        layout.Space(10);

        var UCB = layout.Checkbox("Dropdown");
        var ComboBoxVerticlePanel = layout.VerticalPanel();
        UCB.CheckBox.StateChanged += (CheckBox obj) => { ComboBoxVerticlePanel.Panel.Visible = obj.Checked; ComboBoxVerticlePanel.Panel.Enabled = obj.Checked; DropDownChangeSelected = obj.Checked; };

        var BackGroundColorOverride = ComboBoxVerticlePanel.Checkbox(" > Change Background Colors");
        var DDBackGroundPanel = ComboBoxVerticlePanel.VerticalPanel();

        BackGroundColorOverride.CheckBox.StateChanged += (CheckBox obj) => { DDBackGroundPanel.Panel.Visible = obj.Checked; DDBackGroundPanel.Panel.Enabled = obj.Checked; DDBackgroundChanged = obj.Checked; };

        var DDBase = DDBackGroundPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Base Color");
        DDBase.CustomControl.Value = BaseColor;
        DDBase.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DropDownBackGroundColor = obj.Value; };

        var DDHigh = DDBackGroundPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Highlighted Color");
        DDHigh.CustomControl.Value = HighLightedColor;
        DDHigh.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DropDownBackGroundHighColor = obj.Value; };

        var DDSel = DDBackGroundPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Selected Color");
        DDSel.CustomControl.Value = SelectedColor;
        DDSel.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DropDownBackGroundSelectColor = obj.Value; };

        DDBackGroundPanel.Panel.Visible = false;
        DDBackGroundPanel.Panel.Enabled = false;

        var BoarderColorOverride = ComboBoxVerticlePanel.Checkbox(" > Change Boarder Colors");
        var DDBoarderPanel = ComboBoxVerticlePanel.VerticalPanel();

        BoarderColorOverride.CheckBox.StateChanged += (CheckBox obj) => { DDBoarderPanel.Panel.Visible = obj.Checked; DDBoarderPanel.Panel.Enabled = obj.Checked; DDBoarderChanged = obj.Checked; };

        var DDBBase = DDBoarderPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Base Color");
        DDBBase.CustomControl.Value = BaseColor;
        DDBBase.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DropDownBoarderColor = obj.Value; };

        var DDBHigh = DDBoarderPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Highlighted Color");
        DDBHigh.CustomControl.Value = HighLightedColor;
        DDBHigh.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DropDownBoarderHighColor = obj.Value; };

        var DDBSel = DDBoarderPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Selected Color");
        DDBSel.CustomControl.Value = SelectedColor;
        DDBSel.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DropDownBoarderSelectColor = obj.Value; };

        DDBoarderPanel.Panel.Visible = false;
        DDBoarderPanel.Panel.Enabled = false;

        var ArrowColorOverride = ComboBoxVerticlePanel.Checkbox(" > Change Arrow Colors");
        var DDArrowPanel = ComboBoxVerticlePanel.VerticalPanel();

        ArrowColorOverride.CheckBox.StateChanged += (CheckBox obj) => { DDArrowPanel.Panel.Visible = obj.Checked; DDArrowPanel.Panel.Enabled = obj.Checked; DDArrowChanged = obj.Checked; };

        var DDABase = DDArrowPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Base Color");
        DDABase.CustomControl.Value = BaseColor;
        DDABase.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DropDownArrowColor = obj.Value; };

        var DDAHigh = DDArrowPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Highlighted Color");
        DDAHigh.CustomControl.Value = HighLightedColor;
        DDAHigh.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DropDownArrowHighColor = obj.Value; };

        var DDASel = DDArrowPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Selected Color");
        DDASel.CustomControl.Value = SelectedColor;
        DDASel.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DropDownArrowSelectColor = obj.Value; };

        DDArrowPanel.Panel.Enabled = false;
        DDArrowPanel.Panel.Visible = false;

        ComboBoxVerticlePanel.Panel.Visible = false;
        ComboBoxVerticlePanel.Panel.Enabled = false;

        UCB.CheckBox.StateChanged += (CheckBox obj) =>
        {
            if (!obj.Checked)
            {
                BackGroundColorOverride.CheckBox.Checked = obj.Checked;
                BoarderColorOverride.CheckBox.Checked = obj.Checked;
                ArrowColorOverride.CheckBox.Checked = obj.Checked;
            }
        };

        #endregion

        #region Drop Panel UI Setup
        layout.Space(10);

        var UDP = layout.Checkbox("DropPanel");
        var DropPanelVerticlePanel = layout.VerticalPanel();
        UDP.CheckBox.StateChanged += (CheckBox obj) => { DropPanelVerticlePanel.Panel.Enabled = obj.Checked; DropPanelVerticlePanel.Panel.Visible = obj.Checked; DropPanelChangeSelected = obj.Checked; };

        var HeaderTextColorOverride = DropPanelVerticlePanel.Checkbox(" > Change Header Colors");
        var DPHeaderPanel = DropPanelVerticlePanel.VerticalPanel();

        HeaderTextColorOverride.CheckBox.StateChanged += (CheckBox obj) => { DPHeaderPanel.Panel.Visible = obj.Checked; DPHeaderPanel.Panel.Enabled = obj.Checked; DropPanelColorsChanged = obj.Checked; };


        var DP_TextColor = DPHeaderPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Text Color");
        DP_TextColor.CustomControl.Value = Color.White;
        DP_TextColor.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DP_H_TextColor = obj.Value; };

        var DP_Color = DPHeaderPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Color");
        DP_Color.CustomControl.Value = HighLightedColor;
        DP_Color.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DP_H_Color = obj.Value; };

        var DP_MouseColor = DPHeaderPanel.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Mouse Over Color");
        DP_MouseColor.CustomControl.Value = SelectedColor;
        DP_MouseColor.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DP_H_MouseColor = obj.Value; };

        DPHeaderPanel.Panel.Visible = false;
        DPHeaderPanel.Panel.Enabled = false;


        var ArrowOverridePanel = DropPanelVerticlePanel.Checkbox(" > Change Arrow Textures/Sprites");
        var DPARrowPanel = DropPanelVerticlePanel.VerticalPanel();

        ArrowOverridePanel.CheckBox.StateChanged += (CheckBox obj) => { DPARrowPanel.Panel.Visible = obj.Checked; DPARrowPanel.Panel.Enabled = obj.Checked; DropPanelTextureChanged = obj.Checked; };

        var DPArrowSelectType = DPARrowPanel.ComboBox("   - Texture Type");
        DPArrowSelectType.ComboBox.AddItem("Texture");
        DPArrowSelectType.ComboBox.AddItem("Sprite");
        DPArrowSelectType.ComboBox.AddItem("Color");

        //Texture Panel;
        var dp_vp1 = DPARrowPanel.VerticalPanel();

        var TextureUp = dp_vp1.Custom<FlaxEditor.GUI.AssetPicker>("   - Up Arrow");
        TextureUp.CustomControl.Validator.AssetType = new FlaxEditor.Scripting.ScriptType(typeof(Texture));
        TextureUp.CustomControl.SelectedItemChanged += () => { DP_UpArrow_ImageAsset = TextureUp.CustomControl.Validator.SelectedAsset; };
        var TextureDown = dp_vp1.Custom<FlaxEditor.GUI.AssetPicker>("   - Down Arrow");
        TextureDown.CustomControl.Validator.AssetType = new FlaxEditor.Scripting.ScriptType(typeof(Texture));
        TextureDown.CustomControl.SelectedItemChanged += () => { DP_DownArrow_ImageAsset = TextureDown.CustomControl.Validator.SelectedAsset; };

        dp_vp1.Panel.Visible = false;
        dp_vp1.Panel.Enabled = false;

        //Sprite Panel;
        var dp_vp2 = DPARrowPanel.VerticalPanel();

        var SpriteUp = dp_vp2.Custom<FlaxEditor.GUI.AssetPicker>("   - Up Arrow");
        SpriteUp.CustomControl.Validator.AssetType = new FlaxEditor.Scripting.ScriptType(typeof(SpriteAtlas));
        var SelectedUpSprte = dp_vp2.ComboBox("Select Sprite");
        SelectedUpSprte.ComboBox.SelectedIndexChanged += (ComboBox obj) => { DP_UpArrow_SelectedAtlasID = obj.SelectedIndex; };
        //Upon Sprite Atlas Getting Selected, Updating a ComboBox with Available Sprite's. you dont this from the altas generator :D

        SpriteUp.CustomControl.SelectedItemChanged += () =>
        {
            var SpriteSheet = (SpriteAtlas)SpriteUp.CustomControl.Validator.SelectedAsset;
            DP_UpArrow_ImageAsset = SpriteUp.CustomControl.Validator.SelectedAsset;
            if (SpriteSheet is null)
                return;

            SelectedUpSprte.ComboBox.ClearItems();

            for (int i = 0; i < SpriteSheet.Sprites.Length; i++)
            {
                SelectedUpSprte.ComboBox.AddItem(SpriteSheet.Sprites[i].Name);
            }

            SelectedUpSprte.ComboBox.SelectedIndex = 0;
        };

        var SpriteDown = dp_vp2.Custom<FlaxEditor.GUI.AssetPicker>("   - Down Arrow");
        SpriteDown.CustomControl.Validator.AssetType = new FlaxEditor.Scripting.ScriptType(typeof(SpriteAtlas));
        var SelectedDownSprite = dp_vp2.ComboBox("Select Sprite");
        SelectedDownSprite.ComboBox.SelectedIndexChanged += (ComboBox obj) => { DP_DownArrow_SelectedAtlasID = obj.SelectedIndex; };

        SpriteDown.CustomControl.SelectedItemChanged += () =>
        {
            var SpriteSheet = (SpriteAtlas)SpriteDown.CustomControl.Validator.SelectedAsset;
            DP_DownArrow_ImageAsset = SpriteUp.CustomControl.Validator.SelectedAsset;
            if (SpriteSheet is null)
                return;

            SelectedDownSprite.ComboBox.ClearItems();

            for (int i = 0; i < SpriteSheet.Sprites.Length; i++)
            {
                SelectedDownSprite.ComboBox.AddItem(SpriteSheet.Sprites[i].Name);
            }

            SelectedDownSprite.ComboBox.SelectedIndex = 0;
        };

        dp_vp2.Panel.Visible = false;
        dp_vp2.Panel.Enabled = false;

        var dp_vp3 = DPARrowPanel.VerticalPanel();

        var ColorUpArrow = dp_vp3.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Up Arrow Color");
        ColorUpArrow.CustomControl.Value = Color.White;
        ColorUpArrow.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DP_UpArrow_SolidColor = obj.Value; };

        var ColorDownArrow = dp_vp3.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Down Arrow Color");
        ColorDownArrow.CustomControl.Value = Color.White;
        ColorDownArrow.CustomControl.ColorValueChanged += (ColorValueBox obj) => { DP_DownArrow_SolidColor = obj.Value; };


        dp_vp3.Panel.Visible = false;
        dp_vp3.Panel.Enabled = false;

        DPArrowSelectType.ComboBox.SelectedIndexChanged += (ComboBox obj) =>
        {
            DP_TextureType = (byte)obj.SelectedIndex;

            dp_vp1.Panel.Visible = false;
            dp_vp1.Panel.Enabled = false;
            dp_vp2.Panel.Visible = false;
            dp_vp2.Panel.Enabled = false;
            dp_vp3.Panel.Visible = false;
            dp_vp3.Panel.Enabled = false;


            if (obj.SelectedIndex == 0)
            {
                dp_vp1.Panel.Visible = true;
                dp_vp1.Panel.Enabled = true;


            }
            if (obj.SelectedIndex == 1)
            {

                dp_vp2.Panel.Visible = true;
                dp_vp2.Panel.Enabled = true;
            }
            if (obj.SelectedIndex == 2)
            {
                dp_vp3.Panel.Visible = true;
                dp_vp3.Panel.Enabled = true;
            }
        };

        DPARrowPanel.Panel.Visible = false;
        DPARrowPanel.Panel.Enabled = false;


        DropPanelVerticlePanel.Panel.Visible = false;
        DropPanelVerticlePanel.Panel.Enabled = false;

        #endregion

        #region CheckBox UI Setup

        layout.Space(10);
        var UCKB = layout.Checkbox("Check Box");
        var CheckBoxVerticlePanel = layout.VerticalPanel();
        UCKB.CheckBox.StateChanged += (CheckBox obj) => { CheckBoxVerticlePanel.Panel.Visible = obj.Checked; CheckBoxVerticlePanel.Panel.Enabled = obj.Checked; CheckBoxChangeSelected = obj.Checked; };


        var BoarderCheckboxOverride = CheckBoxVerticlePanel.Checkbox(" > Change Boarder Colors");
        var CBBoarderPaenl = CheckBoxVerticlePanel.VerticalPanel();

        BoarderCheckboxOverride.CheckBox.StateChanged += (CheckBox obj) => { CBBoarderPaenl.Panel.Visible = obj.Checked; CBBoarderPaenl.Panel.Enabled = obj.Checked; CheckBoxBoardChanged = obj.Checked; };

        var CB_Base = CBBoarderPaenl.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Base Color");
        CB_Base.CustomControl.Value = BaseColor;
        CB_Base.CustomControl.ColorValueChanged += (ColorValueBox obj) => { CheckBoxBaseColor = obj.Value; };

        var CB_High = CBBoarderPaenl.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Highlighted Color");
        CB_High.CustomControl.Value = HighLightedColor;
        CB_High.CustomControl.ColorValueChanged += (ColorValueBox obj) => { CheckBoxHighColor = obj.Value; };

        CBBoarderPaenl.Panel.Visible = false;
        CBBoarderPaenl.Panel.Enabled = false;

        var CheckImagesOverride = CheckBoxVerticlePanel.Checkbox(" > Change Arrow Textures/Sprites");
        var CBImagePanel = CheckBoxVerticlePanel.VerticalPanel();

        CheckImagesOverride.CheckBox.StateChanged += (CheckBox obj) => { CBImagePanel.Panel.Visible = obj.Checked; CBImagePanel.Panel.Enabled = obj.Checked; CheckBoxArrowChanged = obj.Checked; };

        var CBImageType = CBImagePanel.ComboBox("   - Texture Type");
        CBImageType.ComboBox.AddItem("Texture");
        CBImageType.ComboBox.AddItem("Sprite");
        CBImageType.ComboBox.AddItem("Color");

        //Texture Panel;
        var cb_vp1 = CBImagePanel.VerticalPanel();

        var TextureCheckedImage = cb_vp1.Custom<FlaxEditor.GUI.AssetPicker>("   - Checked Image");
        TextureCheckedImage.CustomControl.Validator.AssetType = new FlaxEditor.Scripting.ScriptType(typeof(Texture));
        TextureCheckedImage.CustomControl.SelectedItemChanged += () => { CB_ImageAsset = TextureCheckedImage.CustomControl.Validator.SelectedAsset; };

        cb_vp1.Panel.Visible = false;
        cb_vp1.Panel.Enabled = false;

        //Sprite Panel;
        var cb_vp2 = CBImagePanel.VerticalPanel();

        var TextureCheckedSprite = cb_vp2.Custom<FlaxEditor.GUI.AssetPicker>("   - Checked Sprite");
        TextureCheckedSprite.CustomControl.Validator.AssetType = new FlaxEditor.Scripting.ScriptType(typeof(SpriteAtlas));

        var SelectedCheckedSprite = cb_vp2.ComboBox("Select Sprite");
        SelectedCheckedSprite.ComboBox.SelectedIndexChanged += (ComboBox obj) => { CB_SelectedAtlasID = obj.SelectedIndex; };
        //Upon Sprite Atlas Getting Selected, Updating a ComboBox with Available Sprite's. you dont this from the altas generator :D

        TextureCheckedSprite.CustomControl.SelectedItemChanged += () =>
        {
            var SpriteSheet = (SpriteAtlas)TextureCheckedSprite.CustomControl.Validator.SelectedAsset;
            CB_ImageAsset = TextureCheckedSprite.CustomControl.Validator.SelectedAsset;
            if (SpriteSheet is null)
                return;

            SelectedCheckedSprite.ComboBox.ClearItems();

            for (int i = 0; i < SpriteSheet.Sprites.Length; i++)
            {
                SelectedCheckedSprite.ComboBox.AddItem(SpriteSheet.Sprites[i].Name);
            }

            SelectedCheckedSprite.ComboBox.SelectedIndex = 0;
        };



        cb_vp2.Panel.Visible = false;
        cb_vp2.Panel.Enabled = false;
        CBImagePanel.Panel.Visible = false;
        CBImagePanel.Panel.Enabled = false;

        var cb_vp3 = CBImagePanel.VerticalPanel();

        var ChckedColor = cb_vp3.Custom<FlaxEditor.GUI.Input.ColorValueBox>("   - Checked Color");
        ChckedColor.CustomControl.Value = Color.White;
        ChckedColor.CustomControl.ColorValueChanged += (ColorValueBox obj) => { CB_SolidColor = obj.Value; };

        cb_vp3.Panel.Visible = false;
        cb_vp3.Panel.Enabled = false;

        CBImageType.ComboBox.SelectedIndexChanged += (ComboBox obj) =>
        {
            CB_TextureType = (byte)obj.SelectedIndex;

            cb_vp1.Panel.Visible = false;
            cb_vp1.Panel.Enabled = false;
            cb_vp2.Panel.Visible = false;
            cb_vp2.Panel.Enabled = false;
            cb_vp3.Panel.Visible = false;
            cb_vp3.Panel.Enabled = false;


            if (obj.SelectedIndex == 0)
            {
                cb_vp1.Panel.Visible = true;
                cb_vp1.Panel.Enabled = true;


            }
            if (obj.SelectedIndex == 1)
            {

                cb_vp2.Panel.Visible = true;
                cb_vp2.Panel.Enabled = true;
            }
            if (obj.SelectedIndex == 2)
            {
                cb_vp3.Panel.Visible = true;
                cb_vp3.Panel.Enabled = true;
            }
        };

        CBBoarderPaenl.Panel.Visible = false;
        CBBoarderPaenl.Panel.Enabled = false;

        CheckBoxVerticlePanel.Panel.Visible = false;
        CheckBoxVerticlePanel.Panel.Enabled = false;

        #endregion

        #region Bottem Panel Finish UI Setup
        layout.Space(10);

        DisplayInfo = layout.Label("");
        DisplayInfo.Label.AutoHeight = true;
        DisplayInfo.Label.Width = 250;

        var button = layout.Button("");
        button.Button.Text = "Please Select Canvas or Selct All";
        button.Button.Enabled = false;

        UseAll.CheckBox.StateChanged += (CheckBox obj) =>
        {
            if (SelectionBox.ComboBox.Items.Count == 0)
                return;

            ChangingAllCanvas = obj.Checked;
            SelectionBox.ComboBox.Enabled = !ChangingAllCanvas;
            button.Button.Enabled = true;

            if (!obj.Checked && SelectionBox.ComboBox.SelectedIndex == -1)
            {
                SelectionBox.ComboBox.SelectedIndex = 0;
            }
            button.Button.Text = new LocalizedString(obj.Checked ? "Update All Active Canvas's" : $"Update {SelectionBox.ComboBox.Items[SelectionBox.ComboBox.SelectedIndex]} Canvas");
        };

        SelectionBox.ComboBox.SelectedIndexChanged += (ComboBox obj) =>
        {
            PopulateUIControlForCanvas(obj);
            DisplayInfoMessage(
                     $"  Selected Canvas       : {AvailableCanvas[obj.SelectedIndex].Name}\n" +
                     $"  Selected UI Controls  : {SelectedCanvascontrols.Length}"
                     );

            button.Button.Text = new LocalizedString($"Update {SelectionBox.ComboBox.Items[SelectionBox.ComboBox.SelectedIndex]} Canvas");
            button.Button.Enabled = true;
        };

        button.Button.ButtonClicked += (Button obj) => { StartProcessingData(obj); };

        #endregion
    }
    private void StartProcessingData(Button obj)
    {
        if (SelectedCanvascontrols is null && !ChangingAllCanvas)
        {
            DisplayWarning("No Canvas Selected!");
            return;
        }

        if (ChangingAllCanvas)
        {
            SelectedCanvascontrols = Level.GetActors<UIControl>();
        }

        fontsUpdatedCounter = 0;
        buttonsUpdatedCounter = 0;
        SlidersChangedCounter = 0;
        DropDownsChangedCounter = 0;
        DropPanelsChangedCounter = 0;
        CheckBoxChangedCounter = 0;

        UpdateCanvasData();

    }
    void UpdateCanvasData()
    {
        for (int i = 0; i < SelectedCanvascontrols.Length; i++)
        {
            UpdateFont(SelectedCanvascontrols[i].Control);
            UpdateButton(SelectedCanvascontrols[i].Control);
            UpdateSlider(SelectedCanvascontrols[i].Control);
            UpdateDropDown(SelectedCanvascontrols[i].Control);
            UpdateDropPanel(SelectedCanvascontrols[i].Control);
            UpdateCheckBox(SelectedCanvascontrols[i].Control);
        }
        DisplayInfoMessage($"\nFinished Updating Selected Canvas's \nFonts updated : {fontsUpdatedCounter}\nButtons updated : {buttonsUpdatedCounter}\nSliders Updated :{SlidersChangedCounter}\nDropDown Updated :{DropDownsChangedCounter}\nDropPanel Updated :{DropPanelsChangedCounter}\nCheckBox Updated :{CheckBoxChangedCounter}");
    }
    void UpdateCheckBox(Control control)
    {
        if (!CheckBoxChangeSelected || control.GetType() != typeof(CheckBox))
            return;

        CheckBox checkBox = (CheckBox)control;

        bool Altered = false;


        if (CheckBoxBoardChanged)
        {
            checkBox.BorderColor = CheckBoxBaseColor;
            checkBox.BorderColorHighlighted = CheckBoxHighColor;
            Altered = true;
        }
        if (CheckBoxArrowChanged)
        {
            var Checker = checkBox.CheckedImage;
            if (CB_TextureType == 0) // Texture.
            {
                Checker = new TextureBrush((Texture)CB_ImageAsset);
            }
            if (CB_TextureType == 1) //Sprite.
            {
                Checker = new SpriteBrush(new SpriteHandle((SpriteAtlas)CB_ImageAsset, CB_SelectedAtlasID));

            }
            if (CB_TextureType == 2) //Solid Color.
            {
                Checker = new SolidColorBrush(CB_SolidColor);
            }
            checkBox.CheckedImage = Checker;
            Altered = true;
        }

        if (Altered)
            CheckBoxChangedCounter++;

    }
    void UpdateDropPanel(Control control)
    {
        if (!DropPanelChangeSelected || control.GetType() != typeof(DropPanel))
            return;

        DropPanel dropPanel = (DropPanel)control;
        bool Altered = false;


        if (DropPanelColorsChanged)
        {
            dropPanel.HeaderTextColor = DP_H_TextColor;
            dropPanel.HeaderColor = DP_H_Color;
            dropPanel.HeaderColorMouseOver = DP_H_MouseColor;
            Altered = true;
        }

        if (DropPanelTextureChanged)
        {
            var UpArrow = dropPanel.ArrowImageClosed;
            var DownArrow = dropPanel.ArrowImageOpened;

            if (DP_TextureType == 0) // Texture.
            {
                UpArrow = new TextureBrush((Texture)DP_UpArrow_ImageAsset);
                DownArrow = new TextureBrush((Texture)DP_DownArrow_ImageAsset);

            }
            if (DP_TextureType == 1) //Sprite.
            {
                UpArrow = new SpriteBrush(new SpriteHandle((SpriteAtlas)DP_UpArrow_ImageAsset, DP_UpArrow_SelectedAtlasID));
                DownArrow = new SpriteBrush(new SpriteHandle((SpriteAtlas)DP_DownArrow_ImageAsset, DP_DownArrow_SelectedAtlasID));

            }
            if (DP_TextureType == 2) //Solid Color.
            {
                UpArrow = new SolidColorBrush(DP_UpArrow_SolidColor);
                DownArrow = new SolidColorBrush(DP_DownArrow_SolidColor);
            }
            Altered = true;
            dropPanel.ArrowImageClosed = UpArrow;
            dropPanel.ArrowImageOpened = DownArrow;
        }

        if (Altered)
            DropPanelsChangedCounter++;
    }
    void UpdateDropDown(Control control)
    {
        if (!DropDownChangeSelected || control.GetType() != typeof(Dropdown))
            return;

        Dropdown dropdown = (Dropdown)control;
        bool Altered = false;
        if (DDBackgroundChanged)
        {
            dropdown.BackgroundColor = DropDownBackGroundColor;
            dropdown.BackgroundColorHighlighted = DropDownBackGroundHighColor;
            dropdown.BackgroundColorSelected = DropDownBackGroundSelectColor;
            Altered = true;
        }

        if (DDBoarderChanged)
        {
            dropdown.BorderColor = DropDownBoarderColor;
            dropdown.BorderColorHighlighted = DropDownBoarderHighColor;
            dropdown.BorderColorSelected = DropDownBoarderSelectColor;
            Altered = true;
        }

        if (DDArrowChanged)
        {
            dropdown.ArrowColor = DropDownArrowColor;
            dropdown.ArrowColorHighlighted = DropDownArrowHighColor;
            dropdown.ArrowColorSelected = DropDownArrowSelectColor;
            Altered = true;
        }

        if (Altered)
            DropDownsChangedCounter++;
    }
    void UpdateSlider(Control control)
    {
        if (!SliderChangeSelected || control.GetType() != typeof(Slider))
            return;

        bool Altered = false;
        Slider slider = (Slider)control;

        if (TrackColorChanged)
        {
            Altered = true;
            slider.TrackFillLineColor = TrackFillColor;
            slider.TrackLineColor = TrackLineColor;
        }
        if (ThumbColorChanged)
        {
            Altered = true;
            slider.ThumbColor = ThumBColor;
            slider.ThumbColorHighlighted = ThumbHighColor;
            slider.ThumbColorSelected = ThumbSelectColor;
        }
        if (Altered)
            SlidersChangedCounter++;
    }
    void UpdateButton(Control control)
    {
        if (!ButtonChangeSelected || control.GetType() != typeof(Button))
            return;

        Button button = (Button)control;

        bool ButtonAltered = false;

        if (ButtonColorChanged)
        {
            button.BackgroundColor = ButtonBaseColor;
            button.BackgroundColorHighlighted = ButtonHightlightedColor;
            button.BackgroundColorSelected = ButtonSelectedColor;
            ButtonAltered = true;
        }

        if (ButtonImageChanged)
        {
            var background = button.BackgroundBrush;

            if (ImageType == 0) // Texture.
            {
                background = new TextureBrush((Texture)ButtonImageAsset);
            }
            if (ImageType == 1) //Sprite.
            {
                background = new SpriteBrush(new SpriteHandle((SpriteAtlas)ButtonImageAsset, SelectedAtlasID));

            }
            if (ImageType == 2) //Solid Color.
            {
                background = new SolidColorBrush(ButtonSolidColor);
            }
            button.BackgroundBrush = background;

            ButtonAltered = true;
        }

        if (ButtonAltered)
            buttonsUpdatedCounter++;
    }
    void UpdateFont(Control control)
    {
        if (!FontChangeSelected)
            return;

        bool AlatedElement = false;

        if (FontAssetChanged)
        {
            //Update Font's
            UpdateFontComponent(control);
            AlatedElement = true;
        }

        if (FontSizeChanged)
        {
            //Update Size's
            UpdateFontSizeComponent(control);
            AlatedElement = true;
        }
        if (FontColorChanged)
        {
            //Update Color's.
            UpdateFontColorComponent(control);
            AlatedElement = true;
        }

        if (AlatedElement)
            fontsUpdatedCounter++;
    }
    void UpdateFontComponent(Control control)
    {
        FontReference Selected = null;

        if (control.GetType() == typeof(Button))
        {
            Selected = ((Button)control).Font;
        }
        if (control.GetType() == typeof(DropPanel))
        {
            Selected = ((DropPanel)control).HeaderTextFont;
        }
        if (control.GetType() == typeof(Dropdown))
        {
            Selected = ((Dropdown)control).Font;
        }
        if (control.GetType() == typeof(Label))
        {
            Selected = ((Label)control).Font;
        }
        if (control.GetType() == typeof(RichTextBox))
        {
            Selected = ((RichTextBox)control).TextStyle.Font;
        }
        if (control.GetType() == typeof(TextBox))
        {
            Selected = ((TextBox)control).Font;
        }

        if (Selected is not null)
        {
            Selected.Font = (FontAsset)NewFontAsset;
        }
    }
    void UpdateFontSizeComponent(Control control)
    {
        FontReference Selected = null;

        if (control.GetType() == typeof(Button))
        {
            Selected = ((Button)control).Font;
        }
        if (control.GetType() == typeof(DropPanel))
        {
            Selected = ((DropPanel)control).HeaderTextFont;
        }
        if (control.GetType() == typeof(Dropdown))
        {
            Selected = ((Dropdown)control).Font;
        }
        if (control.GetType() == typeof(Label))
        {
            Selected = ((Label)control).Font;
        }
        if (control.GetType() == typeof(RichTextBox))
        {
            Selected = ((RichTextBox)control).TextStyle.Font;
        }
        if (control.GetType() == typeof(TextBox))
        {
            Selected = ((TextBox)control).Font;
        }

        if (Selected is not null)
        {
            if (NewFontSizeType == 0)//Additive
                Selected.Size += NewFontSize;
            else if (NewFontSizeType == 1)//Override
                Selected.Size = NewFontSize;
        }
    }
    void UpdateFontColorComponent(Control control)
    {
        if (control.GetType() == typeof(Button))
        {
            ((Button)control).TextColor = NewFontColor;
        }
        if (control.GetType() == typeof(DropPanel))
        {
            ((DropPanel)control).HeaderTextColor = NewFontColor;
        }
        if (control.GetType() == typeof(Dropdown))
        {
            ((Dropdown)control).TextColor = NewFontColor;
        }
        if (control.GetType() == typeof(Label))
        {
            ((Label)control).TextColor = NewFontColor;
        }
        if (control.GetType() == typeof(RichTextBox))
        {
            var ts = ((RichTextBox)control).TextStyle;
            ts.Color = NewFontColor;
            ((RichTextBox)control).TextStyle = ts;
        }
        if (control.GetType() == typeof(TextBox))
        {
            ((TextBox)control).TextColor = NewFontColor;
        }
    }
    private void DisplayWarning(string Message)
    {
        errorLobel.Label.TextColor = Color.Red;
        errorLobel.Label.Text = $"Error Message : {Message}";
        errorLobel.Control.Height = 20;
        errorLobel.Control.Width = 200;
    }
    void DisplayInfoMessage(string message)
    {
        DisplayInfo.Label.Text = $"Information : \n{message}";
    }
    private void PopulateUIControlForCanvas(ComboBox obj)
    {
        var allControls = Level.GetActors<UIControl>();
        List<UIControl> CanvasBasedControls = new List<UIControl>();

        for (int i = 0; i < allControls.Length; i++)
        {
            if (allControls[i].Control.Root == AvailableCanvas[obj.SelectedIndex].GUI.Root)
            {
                CanvasBasedControls.Add(allControls[i]);
            }
        }
        SelectedCanvascontrols = CanvasBasedControls.ToArray();
    }
    void PopulateCanvasBox(ComboBoxElement box)
    {
        for (int i = 0; i < AvailableCanvas.Length; i++)
        {
            if (!AvailableCanvas[i].IsActive)
                continue;

            string name = $"{AvailableCanvas[i].Name}";
            box.ComboBox.AddItem($"{name}");
        }
    }
    void GetAssets()
    {
        AvailableCanvas = Level.GetActors<UICanvas>();
    }
}

#endif