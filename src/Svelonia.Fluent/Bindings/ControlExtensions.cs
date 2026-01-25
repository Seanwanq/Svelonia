using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Svelonia.Core;

namespace Svelonia.Fluent;



public static class ControlExtensions

{

    /// <summary>

    /// Auto-focuses the control when it is attached to the visual tree.

    /// </summary>

    public static T AutoFocus<T>(this T control)

        where T : Control

    {

        control.AttachedToVisualTree += (s, e) => control.Focus();

        return control;

    }



    /// <summary>

    /// Binds the focus state of the control to a State of boolean.

    /// When the state becomes true, the control is focused.

    /// </summary>

    public static T BindFocus<T>(this T control, State<bool> focusState) where T : Control

    {

        Action<bool> update = focused =>

        {

            if (focused) 

            {

                if (!control.IsLoaded)

                {

                    // If not loaded yet, wait for attachment

                    EventHandler<RoutedEventArgs>? handler = null;

                    handler = (s, e) => {

                        control.Loaded -= handler;

                        if ((bool)focusState.Value) 

                        {

                            Avalonia.Threading.Dispatcher.UIThread.Post(() => {

                                if (control.IsVisible) control.Focus();

                            }, Avalonia.Threading.DispatcherPriority.Input);

                        }

                    };

                    control.Loaded += handler;

                    return;

                }



                // If already loaded, focus immediately

                Avalonia.Threading.Dispatcher.UIThread.Post(() => {

                    if (control.IsVisible) control.Focus();

                }, Avalonia.Threading.DispatcherPriority.Input);

            }

        };



        focusState.OnChange += update;

        control.RegisterDisposable(new AnonymousDisposable(() => focusState.OnChange -= update));



        if (focusState.Value) update(true);



        return control;

    }



    private class AnonymousDisposable(Action dispose) : IDisposable

    {

        public void Dispose() => dispose();

    }



    /// <summary>

    /// Binds a TextBox to a BufferedState, automatically handling Enter (commit) and Escape (reset).

    /// </summary>

    public static TextBox BindBufferedText(

        this TextBox control,

        BufferedState<string> buffer,

        State<bool>? isEditing = null

    )

    {

        // Rely on generated BindText (since we are using it on TextBox)

        // If BindText is not generated, we'll fix it in the generator.

        control.Bind(TextBox.TextProperty, buffer.ToBinding());



        control.OnKey(

            "Enter",

            ()

            =>

            {

                buffer.Commit();

                if (isEditing != null)

                    isEditing.Value = false;

            },

            handled: true

        );



        control.OnKey(

            "Escape",

            ()

            =>

            {

                buffer.Reset();

                if (isEditing != null)

                    isEditing.Value = false;

            },

            handled: true

        );



        // Optional: Sync buffer when editing starts

        if (isEditing != null)

        {

            isEditing.OnChange += editing =>

            {

                if (editing)

                    buffer.Reset();

            };

        }



        return control;

    }
    public static T SveBindIsVisible<T>(this T control, State<bool> state) where T : Control
    {
        control.Bind(Visual.IsVisibleProperty, state.ToBinding());
        return control;
    }

    public static T SveBindOpacity<T>(this T control, State<double> state) where T : Control
    {
        control.Bind(Visual.OpacityProperty, state.ToBinding());
        return control;
    }

    public static T SveBindWidth<T>(this T control, State<double> state) where T : Control
    {
        control.Bind(Layoutable.WidthProperty, state.ToBinding());
        return control;
    }

    public static T SveBindHeight<T>(this T control, State<double> state) where T : Control
    {
        control.Bind(Layoutable.HeightProperty, state.ToBinding());
        return control;
    }

    public static T SveBindBackground<T>(this T control, State<IBrush> state) where T : Control
    {
        var prop = control is TemplatedControl ? TemplatedControl.BackgroundProperty
                 : control is Border ? Border.BackgroundProperty
                 : control is Panel ? Panel.BackgroundProperty
                 : null;
        if (prop != null) control.Bind(prop, state.ToBinding());
        return control;
    }

    public static T SveSetBorderBrush<T>(this T control, IBrush? value) where T : Control
    {
        if (control is Border b) b.BorderBrush = value;
        else if (control is TemplatedControl tc) tc.BorderBrush = value;
        return control;
    }

    /// <summary>
    /// Binds any AvaloniaProperty to a State with a fluent return.
    /// </summary>
    public static T SveBindState<T, V>(this T control, AvaloniaProperty<V> property, State<V> state)
        where T : Control
    {
        control.Bind(property, state.ToBinding());
        return control;
    }

    public static T SveBindTwoWay<T, V>(this T control, AvaloniaProperty<V> property, State<V> state)
        where T : Control
    {
        var binding = new Binding("Value") { Source = state, Mode = BindingMode.TwoWay };
        control.Bind(property, binding);
        return control;
    }

}