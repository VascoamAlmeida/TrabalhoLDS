const { test, expect } = require("@playwright/test");

test("Account – Registo de novo utilizador", async ({ page }) => {
    const timestamp = Date.now();
    const email = `playwright${timestamp}@email.com`;
    const password = "Teste123!";

    await page.goto("/register");

    // Preenche formulário de registo
    await page.fill('input[name="Email"]', email);
    await page.fill('input[name="Password"]', password);
    await page.getByRole("button", { name: "Registar" }).click();

    // Após registo, controller faz login automático e redireciona para Profile
    await page.waitForURL("**/profile");

    // Verifica se fez login
    await expect(page.getByText("Sair")).toBeVisible();
});
