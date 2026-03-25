<#import "template.ftl" as layout>

<@layout.registrationLayout displayInfo=false displayMessage=true; section>

    <#if section == "header">
        <div class="tp-header">
            <img src="${url.resourcesPath}/images/logo.png" alt="Logo" class="tp-logo"/>
            <h1>TestPlatform</h1>
        </div>
    </#if>

    <#if section == "form">
        <form id="kc-form-login" action="${url.loginAction}" method="post" class="tp-form">

            <div class="tp-input">
                <input type="text" name="username" placeholder="Табельный номер" autofocus />
            </div>

            <div class="tp-input">
                <input type="password" name="password" placeholder="Пароль" />
            </div>

            <div class="tp-actions">
                <button type="submit" class="tp-btn-primary">Войти</button>
            </div>

        </form>
    </#if>

</@layout.registrationLayout>

<link rel="stylesheet" href="${url.resourcesPath}/css/styles.css"/>
