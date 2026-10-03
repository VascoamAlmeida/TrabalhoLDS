const { test, expect } = require("@playwright/test");

const timestamp = Date.now();
const email = `playwright${timestamp}@email.com`;
const password = "Teste123!";
async function register(page) {

    await page.goto("/register");

    /// Preenche os campos de email e pass
    await page.fill('input[name="Email"]', email);
    await page.fill('input[name="Password"]', password);
    await page.getByRole("button", { name: "Registar" }).click();
}

// função auxiliar de login como cliente de teste
async function login(page) {
    await page.goto("/login");

    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', password);

    await page.getByRole("button", { name: "Entrar" }).click();
}

test("Account – Login", async ({ page }) => {
    await register(page);

    await expect(page.getByText("Sair")).toBeVisible();

    await page.getByRole("button", { name: "Sair" }).click(); // clicar no botao de sair

    await login(page);

    // deve redirecionar para /profile
    await page.waitForURL("**/profile");

    // verifica que aparece a secção Dados Pessoais
    await expect(page.getByText("Dados Pessoais")).toBeVisible();

    // Verifica se fez login
    await expect(page.getByText("Sair")).toBeVisible();
});