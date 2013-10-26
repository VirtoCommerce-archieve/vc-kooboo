﻿VirtoCheckout = function () {
};
VirtoCheckout.prototype.constructor = VirtoCheckout.prototype;

VirtoCheckout.prototype = {
    initialize: function () {
    },

    updateLoading: function (placeholderId) {
        var placeholder = $("#" + placeholderId);
        placeholder.html("<div class=\"loading-ajax\">&nbsp;</div>");
    },

    updateCart: function () {
        var placeholder = $("#checkout-cart");
        this.updateLoading("checkout-cart");
        $.get(VirtoCommerce.url('/checkout/DisplayCart'), {}, function (data) { console.log('updatedcart'); placeholder.html(data); }, "html");
    },

    updateShipments: function () {
        var placeholder = $("#shipping-methods");

        var checkout = this;
        this.updateLoading("shipping-methods");

        $.ajax({
            type: 'GET',
            url: VirtoCommerce.url('/checkout/DisplayShipments'),
            success: function (data) {
                placeholder.html(data);

                $('input[name=ShippingMethod]').each(function () {
                    if ($(this).is(":checked")) {
                        checkout.updatePayments($(this).val());
                    }
                    $(this).bind("click", function () {
                        checkout.shipmentChanged(this);
                    });
                });

                checkout.updateValidation();
            }
        });
    },

    updatePayments: function (shippingMethod) {
        if (shippingMethod == null || shippingMethod == 'undefined' || shippingMethod.length == 0) {
            return;
        }

        var placeholder = $("#payment-methods");
        this.updateLoading("payment-methods");
        var checkout = this;

        $.ajax({
            type: 'GET',
            url: VirtoCommerce.url('/checkout/DisplayPayments?shippingMethod=' + shippingMethod),
            success: function (data) {
                placeholder.html(data);

                $('input[name=PaymentMethod]').each(function () {
                    if (!this.checked) {
                        VirtoCommerce.disableAll($("#container_payment_method_" + this.value));
                    }
                    $(this).bind("click", function () {
                        checkout.paymentChanged(this);
                    });
                });

                checkout.updateValidation();
            }
        });
    },

    shipmentChanged: function (shipment) {
        var sm = $(shipment);
        this.updatePayments(sm.val());
        //this.submitChanges();
    },

    paymentChanged: function (payment) {
        var pm = $(payment);
        //this.updateShipments(pm.val());
        $('dd[name=container_payment_method]').each(function () { $(this).hide(); });
        VirtoCommerce.disableAll($("dd[name=container_payment_method]"));
        $("#container_payment_method_" + pm.val()).fadeIn();
        VirtoCommerce.enableAll($("#container_payment_method_" + pm.val()));
        this.submitChanges();
    },

    bindAccount: function () {
        this.toggleAccount();
    },

    toggleAccount: function () {
        if ($("#CreateAccount").is(':checked')) {
            $("#onestepcheckout-li-password").fadeIn();
            VirtoCommerce.enableAll($("#onestepcheckout-li-password"));
        }
        else {
            $("#onestepcheckout-li-password").hide();
            VirtoCommerce.disableAll($("#onestepcheckout-li-password"));
        }
    },

    bindGiftMessage: function () {
        this.toggleGiftMessage();
    },

    toggleGiftMessage: function () {
        if ($("#IncludeGiftMessage").is(':checked')) {
            $("#allow-gift-message-container").fadeIn();
            VirtoCommerce.enableAll($("#allow-gift-message-container"));

        }
        else {
            $("#allow-gift-message-container").hide();
            VirtoCommerce.disableAll($("#allow-gift-message-container"));
        }
    },

    bindAddresses: function () {
        this.toggleAddress();
    },

    toggleAddress: function () {
        if ($("#UseForShipping").is(':checked')) {
            $("#shipping_address").hide();
            VirtoCommerce.disableAll($("#shipping_address"));
        }
        else {
            $("#shipping_address").fadeIn();
            VirtoCommerce.enableAll($("#shipping_address"));
        }
    },

    bindFeedback: function () {
        var checkout = this;
        $('#id_feedback').change(function () {
            checkout.toggleFeedback();
        });
        this.toggleFeedback();
    },

    toggleFeedback: function () {
        if ($("#id_feedback").val() == 'other')
            $("#id_feedback_freetext_div").fadeIn();
        else
            $("#id_feedback_freetext_div").hide();
    },

    showHearAboutOther: function () {
        if ($("#id_feedback").val() == "freetext")
            $("#FeedbackMessage").fadeIn();
        else
            $("#FeedbackMessage").hide();
    },

    addressChanged: function (newaddress, type) {
        var selector = $("#billing_address_list");

        if (type == 'shipping')
            selector = $("#shipping_address_list");

        if (newaddress)
            selector.fadeIn();
        else
            selector.hide();
    },

    submitChanges: function () {
        var form = $('#onestepcheckout-form');
        var placeholder = $("#checkout-cart");
        this.updateLoading("checkout-cart");

        $.ajax({
            type: "POST",
            url: VirtoCommerce.url('/checkout/SubmitChanges'),
            data: form.serialize(),
            dataType: 'html',
            success: function (data) { placeholder.html(data); }
        });
    },

    submitCheckout: function () {
        var form = $("#onestepcheckout-form");
        //alert(form.valid());
        //form.validate();
        if (form.valid()) {
            form.submit();
        }
    },

    updateValidation: function () {
        var form = $("#onestepcheckout-form");
        form.unbind();
        form.data("validator", null);

        // Check document for changes
        $.validator.unobtrusive.parse(form);

        // Re add validation with changes
        form.validate(form.data("unobtrusiveValidation").options);
    }
};
var VirtoCheckout = new VirtoCheckout();
