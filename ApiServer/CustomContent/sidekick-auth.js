(function () {
    $(function () {
        console.log('***1here is my custom content!');
        var basicAuthUI =
            '<div class="input"><input placeholder="Auth key, e.g. Basic 3452ewrfadfa=" id="basic_auth_value" name="basic_auth_value" type="text"/></div>';
        $(basicAuthUI).insertBefore('#api_selector div.input:last-child');
        $("#input_apiKey").hide();

        $('#basic_auth_value').change(addAuthorization);
        
    });

    function addAuthorization() {
        var basicAuthValue = $('#basic_auth_value').val();
        
        if (basicAuthValue && basicAuthValue.trim() != "") {
            swaggerUi.api.clientAuthorizations.add("key", new SwaggerClient.ApiKeyAuthorization("Authorization", basicAuthValue, "header"));
        }
    }
})();