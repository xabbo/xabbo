<Application
  xmlns="https://github.com/avaloniaui"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
  x:Class="Xabbo.App"
  xmlns:local="using:Xabbo"
  xmlns:sty="using:FluentAvalonia.Styling"
  xmlns:ui="using:FluentAvalonia.UI.Controls"
  xmlns:ic="using:FluentIcons.Avalonia.Fluent"
  xmlns:img="clr-namespace:AsyncImageLoader;assembly=AsyncImageLoader.Avalonia"
  xmlns:services="using:Xabbo.Avalonia.Services"
  xmlns:c="using:Xabbo.Converters"
  RequestedThemeVariant="Dark"
>
    <Application.DataTemplates>
      <local:ViewLocator/>
    </Application.DataTemplates>

    <Application.Styles>
      <sty:FluentAvaloniaTheme  />
      <StyleInclude Source="avares://FluentAvalonia.ProgressRing/Styling/Controls/ProgressRing.axaml" />
      <StyleInclude Source="avares://AsyncImageLoader.Avalonia/AdvancedImage.axaml" />
      <Style Selector="img|AdvancedImage">
        <Setter Property="Template">
          <ControlTemplate>
            <Grid/>
          </ControlTemplate>
        </Setter>
        <Style Selector="^.spinner">
          <Setter Property="Template">
            <ControlTemplate>
              <Grid>
                <ui:ProgressRing
                  HorizontalAlignment="Center"
                  VerticalAlignment="Center"
                  Width="24" Height="24"
                  IsVisible="{TemplateBinding IsLoading}"
                />
              </Grid>
            </ControlTemplate>
          </Setter>
        </Style>
      </Style>

      <Style Selector=":is(Control).origins :is(Control).modern">
        <Setter Property="IsVisible" Value="False" />
      </Style>

      <Style Selector="SelectableTextBlock">
        <Setter Property="SelectionBrush" Value="Purple" />
      </Style>

      <Style Selector="DataGrid DataGridCell:current /template/ Grid#FocusVisual">
        <Setter Property="IsVisible" Value="False" />
      </Style>

      <Style Selector="ic|SymbolIcon.filled">
        <Setter Property="IconVariant" Value="Filled" />
      </Style>

      <Style Selector="RadioButton">
        <Setter Property="Margin" Value="0,0,5,0" />
      </Style>

      <!-- Slimmer tab items -->
      <Style Selector="TabItem">
        <Setter Property="MinHeight" Value="40" />
        <Setter Property="FontSize" Value="14" />
      </Style>

      <!-- DataGrid -->
      <Style Selector="TextBlock#CellTextBlock">
        <Setter Property="TextTrimming" Value="CharacterEllipsis" />
      </Style>
      <Style Selector="DataGridColumnHeader:sortascending /template/ ui|FontIcon#SortIcon">
        <Setter Property="FontSize" Value="10" />
        <Setter Property="Margin" Value="0" />
      </Style>
      <Style Selector="DataGridColumnHeader:sortdescending /template/ ui|FontIcon#SortIcon">
        <Setter Property="FontSize" Value="10" />
        <Setter Property="Margin" Value="0" />
      </Style>
    </Application.Styles>

  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <ResourceInclude Source="Controls/Loading.axaml" />
      </ResourceDictionary.MergedDictionaries>

      <!-- Session -->
      <!-- Fix: make checkbox padding consistent across different platforms -->
      <Thickness x:Key="CheckBoxPadding">8,5,8,5</Thickness>

      <c:HabboStringConverter x:Key="HabboStringConverter"/>
      <c:MultiValueConverter x:Key="MultiValueConverter"/>
      <x:Boolean x:Key="IsConnecting">True</x:Boolean>
      <x:Boolean x:Key="IsConnected">False</x:Boolean>
      <x:Boolean x:Key="IsOrigins">False</x:Boolean>
      <x:Boolean x:Key="IsModern">False</x:Boolean>
      <x:String x:Key="AppStatus">Waiting for connection...</x:String>

      <Color x:Key="RadioButtonCheckGlyphFill">#5000AA</Color>
      <Color x:Key="RadioButtonCheckGlyphFillPointerOver">#5000AA</Color>
      <Color x:Key="RadioButtonCheckGlyphFillPressed">#5000AA</Color>
      <Color x:Key="RadioButtonCheckGlyphFillDisabled">#333333</Color>

      <!-- Fix: make checkbox padding consistent across different platforms -->
      <Thickness x:Key="CheckBoxPadding">8,5,8,5</Thickness>

      <c:HabboStringConverter x:Key="HabboStringConverter"/>
      <c:MultiValueConverter x:Key="MultiValueConverter"/>
      <c:BoolToOpacityConverter x:Key="BoolToOpacityConverter"/>
      <c:HumanizerConverter x:Key="HumanizerConverter"/>
      <c:WhitespaceNewlineConverter x:Key="WhitespaceNewlineConverter" />
      <c:GtConverter x:Key="GtConverter" />
      <c:EqualityConverter x:Key="EqConverter"/>
      <c:InequalityConverter x:Key="NeqConverter"/>

    </ResourceDictionary>
  </Application.Resources>
</Application>