namespace AdMaiora.Bugghy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Runtime;
    using Android.Util;
    using Android.Views;
    using Android.Widget;    

    using AdMaiora.AppKit.UI;

    #pragma warning disable CS4014
    public class Registration1Fragment : AdMaiora.AppKit.UI.App.Fragment
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private string _email;
        private string _password;

        // This flag check if we are already calling the login REST service
        private bool _isRegisteringUser;
        // This cancellation token is used to cancel the rest login request
        private CancellationTokenSource _cts;

        #endregion

        #region Widgets

        [Widget]
        private EditText PasswordText;

        #endregion

        #region Constructors and Destructors

        public Registration1Fragment()
        {
        }

        #endregion

        #region Properties
        #endregion

        #region Fragment Methods

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _email = this.Arguments.GetString("Email");
        }

        public override void OnCreateView(LayoutInflater inflater, ViewGroup container)
        {
            base.OnCreateView(inflater, container);

            #region Desinger Stuff

            SetContentView(Resource.Layout.FragmentRegistration1, inflater, container);            

            SlideUpToShowKeyboard();

            this.HasOptionsMenu = true;

            #endregion

            this.ActionBar.Show();

            this.PasswordText.Text = _password;
            this.PasswordText.RequestFocus();
            this.PasswordText.EditorAction += PasswordText_EditorAction;
        }

        public override void OnStart()
        {
            base.OnStart();

            this.PasswordText.RequestUserInput();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.DismissKeyboard();
                    this.FragmentManager.PopBackStack();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            if (_cts != null)
                _cts.Cancel();

            this.PasswordText.EditorAction -= PasswordText_EditorAction;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Methods

        public void RegisterUser()
        {
            if(ValidateInput())
            {
                DismissKeyboard();

                AppController.Utility.ExecuteDelayedAction(300, default(System.Threading.CancellationToken),
                    () =>
                    {

                        if (_isRegisteringUser)
                            return;

                        _isRegisteringUser = true;

                        _password = this.PasswordText.Text;

                        // Prevent user form tapping views while logging
                        ((MainActivity)this.Activity).BlockUI();

                        // Create a new cancellation token for this request                
                        _cts = new CancellationTokenSource();
                        AppController.RegisterUser(_cts,
                            _email,
                            _password,
                            // Service call success                 
                            (data) =>
                            {
                                var f = new RegistrationDoneFragment();
                                this.FragmentManager.PopBackStack("BeforeRegistration0Fragment", (int)PopBackStackFlags.Inclusive);
                                this.FragmentManager.BeginTransaction()
                                    .AddToBackStack("BeforeRegistrationDoneFragment")
                                    .Replace(Resource.Id.ContentLayout, f, "RegistrationDoneFragment")
                                    .Commit();
                            },
                            // Service call error
                            (error) =>
                            {
                                this.PasswordText.RequestFocus();

                                Toast.MakeText(this.Activity.Application, error, ToastLength.Long).Show();
                            },
                            // Service call finished 
                            () =>
                            {
                                _isRegisteringUser = false;

                                // Allow user to tap views
                                ((MainActivity)this.Activity).UnblockUI();
                            });
                    });
            }
        }

        public bool ValidateInput()
        {
            var validator = new WidgetValidator()
                .AddValidator(() => this.PasswordText.Text, WidgetValidator.IsNotNullOrEmpty, "Please insert a password.")
                .AddValidator(() => this.PasswordText.Text, WidgetValidator.IsPasswordMin8, "Your password is not valid!");

            string errorMessage;
            if (!validator.Validate(out errorMessage))
            {
                Toast.MakeText(this.Activity.Application, errorMessage, ToastLength.Long).Show();
                return false;
            }

            return true;
        }

        #endregion

        #region Event Handlers

        private void PasswordText_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == Android.Views.InputMethods.ImeAction.Done)
            {
                e.Handled = true;
                RegisterUser();                
            }
            else
            {
                e.Handled = false;
            }
        }

        #endregion
    }
}