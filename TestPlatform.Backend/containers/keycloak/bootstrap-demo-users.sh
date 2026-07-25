#!/usr/bin/env bash
set -euo pipefail

KCADM="/opt/keycloak/bin/kcadm.sh"
SERVER_URL="http://keycloak:8080"
REALM="test-platform"

: "${KEYCLOAK_ADMIN:?KEYCLOAK_ADMIN must be set}"
: "${KEYCLOAK_ADMIN_PASSWORD:?KEYCLOAK_ADMIN_PASSWORD must be set}"
: "${DEMO_ADMIN_PASSWORD:?DEMO_ADMIN_PASSWORD must be set}"
: "${DEMO_TEACHER_PASSWORD:?DEMO_TEACHER_PASSWORD must be set}"
: "${DEMO_EMPLOYEE_PASSWORD:?DEMO_EMPLOYEE_PASSWORD must be set}"

until "${KCADM}" config credentials \
    --server "${SERVER_URL}" \
    --realm master \
    --user "${KEYCLOAK_ADMIN}" \
    --password "${KEYCLOAK_ADMIN_PASSWORD}" >/dev/null 2>&1; do
    echo "Waiting for Keycloak..."
    sleep 2
done

"${KCADM}" update "authentication/required-actions/VERIFY_PROFILE" \
    -r "${REALM}" \
    -s "enabled=false" \
    -s "defaultAction=false" >/dev/null

"${KCADM}" update "realms/${REALM}" \
    -s "loginTheme=mytheme" \
    -s "internationalizationEnabled=true" \
    -s 'supportedLocales=["ru"]' \
    -s "defaultLocale=ru" >/dev/null

ensure_employee_number_token_claim() {
    local scope_id=""
    local mapper_id=""
    local public_client_id=""
    local assigned_scope_id=""

    while IFS=, read -r candidate_scope_id candidate_scope_name; do
        if [[ "${candidate_scope_name}" == "test-platform-api" ]]; then
            scope_id="${candidate_scope_id}"
            break
        fi
    done < <("${KCADM}" get client-scopes \
        -r "${REALM}" \
        --fields id,name \
        --format csv \
        --noquotes)

    if [[ -z "${scope_id}" || "${scope_id}" == "id" ]]; then
        "${KCADM}" create client-scopes \
            -r "${REALM}" \
            -s "name=test-platform-api" \
            -s "protocol=openid-connect" \
            -s 'attributes={"include.in.token.scope":"true","display.on.consent.screen":"false"}' >/dev/null

        while IFS=, read -r candidate_scope_id candidate_scope_name; do
            if [[ "${candidate_scope_name}" == "test-platform-api" ]]; then
                scope_id="${candidate_scope_id}"
                break
            fi
        done < <("${KCADM}" get client-scopes \
            -r "${REALM}" \
            --fields id,name \
            --format csv \
            --noquotes)
    fi

    while IFS=, read -r candidate_mapper_id candidate_mapper_name; do
        if [[ "${candidate_mapper_name}" == "employee_number" ]]; then
            mapper_id="${candidate_mapper_id}"
            break
        fi
    done < <("${KCADM}" get "client-scopes/${scope_id}/protocol-mappers/models" \
        -r "${REALM}" \
        --fields id,name \
        --format csv \
        --noquotes)

    if [[ -z "${mapper_id}" ]]; then
        "${KCADM}" create "client-scopes/${scope_id}/protocol-mappers/models" \
            -r "${REALM}" \
            -s "name=employee_number" \
            -s "protocol=openid-connect" \
            -s "protocolMapper=oidc-usermodel-attribute-mapper" \
            -s 'config={"user.attribute":"employee_number","claim.name":"employee_number","jsonType.label":"String","access.token.claim":"true","id.token.claim":"true","userinfo.token.claim":"true"}' >/dev/null
    else
        "${KCADM}" update "client-scopes/${scope_id}/protocol-mappers/models/${mapper_id}" \
            -r "${REALM}" \
            -s 'config={"user.attribute":"employee_number","claim.name":"employee_number","jsonType.label":"String","access.token.claim":"true","id.token.claim":"true","userinfo.token.claim":"true"}' >/dev/null
    fi

    mapper_id=""
    while IFS=, read -r candidate_mapper_id candidate_mapper_name; do
        if [[ "${candidate_mapper_name}" == "subject" ]]; then
            mapper_id="${candidate_mapper_id}"
            break
        fi
    done < <("${KCADM}" get "client-scopes/${scope_id}/protocol-mappers/models" \
        -r "${REALM}" \
        --fields id,name \
        --format csv \
        --noquotes)

    if [[ -z "${mapper_id}" ]]; then
        "${KCADM}" create "client-scopes/${scope_id}/protocol-mappers/models" \
            -r "${REALM}" \
            -s "name=subject" \
            -s "protocol=openid-connect" \
            -s "protocolMapper=oidc-usermodel-property-mapper" \
            -s 'config={"user.attribute":"id","claim.name":"sub","jsonType.label":"String","access.token.claim":"true","id.token.claim":"true","userinfo.token.claim":"true"}' >/dev/null
    fi

    public_client_id="$("${KCADM}" get clients \
        -r "${REALM}" \
        -q "clientId=public-client" \
        --fields id \
        --format csv \
        --noquotes | tail -n 1 | tr -d '\r')"
    assigned_scope_id="$("${KCADM}" get "clients/${public_client_id}/default-client-scopes" \
        -r "${REALM}" \
        --fields id \
        --format csv \
        --noquotes | tr -d '\r' | grep -Fx "${scope_id}" || true)"

    if [[ -n "${public_client_id}" && "${public_client_id}" != "id" && -z "${assigned_scope_id}" ]]; then
        "${KCADM}" update "clients/${public_client_id}/default-client-scopes/${scope_id}" \
            -r "${REALM}" \
            -n >/dev/null
    fi
}

ensure_employee_number_token_claim

ensure_user() {
    local username="$1"
    local employee_number="$2"
    local password="$3"
    local role="$4"
    local user_id

    user_id="$("${KCADM}" get users \
        -r "${REALM}" \
        -q "username=${username}" \
        --fields id \
        --format csv \
        --noquotes | tail -n 1 | tr -d '\r')"

    if [[ -z "${user_id}" || "${user_id}" == "id" ]]; then
        "${KCADM}" create users \
            -r "${REALM}" \
            -s "username=${username}" \
            -s "enabled=true" \
            -s "attributes={\"employee_number\":[\"${employee_number}\"]}" >/dev/null

        user_id="$("${KCADM}" get users \
            -r "${REALM}" \
            -q "username=${username}" \
            --fields id \
            --format csv \
            --noquotes | tail -n 1 | tr -d '\r')"
    else
        "${KCADM}" update "users/${user_id}" \
            -r "${REALM}" \
            -s "enabled=true" \
            -s "attributes={\"employee_number\":[\"${employee_number}\"]}" >/dev/null
    fi

    "${KCADM}" set-password \
        -r "${REALM}" \
        --userid "${user_id}" \
        --new-password "${password}" >/dev/null

    "${KCADM}" add-roles \
        -r "${REALM}" \
        --uid "${user_id}" \
        --rolename "${role}" >/dev/null

    echo "Keycloak demo account '${username}' is ready with role '${role}'."
}

ensure_user "demo.admin" "DEMO-ADMIN" "${DEMO_ADMIN_PASSWORD}" "Admin"
ensure_user "demo.teacher" "DEMO-TEACHER-LOGIN" "${DEMO_TEACHER_PASSWORD}" "Teacher"
ensure_user "demo.employee" "DEMO-EMPLOYEE" "${DEMO_EMPLOYEE_PASSWORD}" "Employee"

echo "Keycloak demo account bootstrap completed."
