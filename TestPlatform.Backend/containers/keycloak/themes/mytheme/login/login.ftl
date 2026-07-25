<#import "template.ftl" as layout>

<@layout.registrationLayout displayInfo=false displayMessage=true; section>
    <#if section == "header">
        <div class="tp-brand">
            <div class="tp-logo-wrap">
                <img src="${url.resourcesPath}/images/logo.png" alt="" class="tp-logo"/>
            </div>
            <span class="tp-eyebrow">Корпоративное обучение</span>
            <h1>TestPlatform</h1>
            <p>Войдите в личный кабинет сотрудника</p>
        </div>
    <#elseif section == "form">
        <form id="kc-form-login" action="${url.loginAction}" method="post">
            <div class="tp-field">
                <label for="username">Логин</label>
                <input
                    id="username"
                    name="username"
                    type="text"
                    value="${(login.username!'')}"
                    autocomplete="username"
                    placeholder="Введите логин"
                    autofocus
                    aria-invalid="<#if messagesPerField.existsError('username','password')>true</#if>"
                />
            </div>

            <div class="tp-field">
                <label for="password">Пароль</label>
                <input
                    id="password"
                    name="password"
                    type="password"
                    autocomplete="current-password"
                    placeholder="Введите пароль"
                    aria-invalid="<#if messagesPerField.existsError('username','password')>true</#if>"
                />
            </div>

            <#if messagesPerField.existsError('username','password')>
                <div class="tp-field-error" role="alert">
                    Неверный логин или пароль
                </div>
            </#if>

            <button id="kc-login" name="login" type="submit" class="tp-submit">
                Войти
            </button>
        </form>

        <p class="tp-help">
            Данные для входа выдаёт администратор организации
        </p>
    </#if>
</@layout.registrationLayout>
