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

test("Account – Atualizar dados de perfil do cliente", async ({ page }) => {
    await register(page);

    await expect(page.getByText("Sair")).toBeVisible();

    await page.getByRole("button", { name: "Sair" }).click(); // clicar no botao de sair

    await login(page);

    // deve redirecionar para /profiled
    await page.waitForURL("**/profile");

    // Preenche alguns campos do Client
    await page.fill('input[name="Client.FirstName"]', "Playwright Teste");
    await page.fill('input[name="Client.LastName"]', "Playwright");
    await page.fill('input[name="Client.Address"]', "Rua dos Testes 123");
    await page.fill('input[name="Client.PostalCode"]', "1000-001");
    await page.fill('input[name="Client.PhoneNumber"]', "911512461");
    await page.fill('input[name="Client.NIF"]', "333789789");
    
    // Submete o formulário de perfil
    await page.getByRole("button", { name: "Guardar Altera\u00E7\u00F5es" }).click();

    await page.waitForURL("**/profile"); // há redirect

    await page.waitForLoadState("networkidle");

    // Verifica se o primeiro nome ficou com o valor que introduzimos
    await expect(page.locator('input[name="Client.FirstName"]')).toHaveValue("Playwright Teste");
});
