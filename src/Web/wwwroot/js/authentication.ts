const passwordInput = $('#Password')
const passwordToggle = $('#password-toggle')

passwordToggle.on('click', function () {
    const type = passwordInput.attr('type') === 'password' ? 'text' : 'password'
    passwordInput.attr('type', type)
    passwordToggle.toggleClass('fa-eye-slash')
})
