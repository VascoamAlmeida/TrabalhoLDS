const { test, expect } = require("@playwright/test");

const timestamp = Date.now();
const email = process.env.E2E_ADMIN_EMAIL;
const password = process.env.E2E_ADMIN_PASSWORD;

test.skip(!email || !password, "Set E2E_ADMIN_EMAIL and E2E_ADMIN_PASSWORD to run administrator E2E tests.");

async function register(page) {
    await login(page);
}

// função auxiliar de login como cliente de teste
async function login(page) {
    await page.goto("/login");

    await page.fill('input[name="email"]', email);
    await page.fill('input[name="password"]', password);

    await page.getByRole("button", { name: "Entrar" }).click();
}

test("Category – Criar nova categoria", async ({ page }) => {
    await register(page);

    await expect(page.getByText("Sair")).toBeVisible();

    await page.getByRole("button", { name: "Sair" }).click(); // clicar no botao de sair

    await login(page);

    await page.goto("/category-list");
    await page.waitForLoadState("networkidle");

    // 1. Clica em "Adicionar" para abrir o modal
    await page.click("text=Adicionar");

    // 2. Garante que o modal de criação apareceu
    const modal = page.locator("#createModal");
    await expect(modal).toBeVisible();

    const inputName = modal.locator('input[name="Name"]');
    const inputDesc = modal.locator('input[name="Description"]');
    await expect(inputName).toBeVisible();

    // 4. Preenche os campos com um nome único
    const timestamp = Date.now();
    const categoriaNome = `Categoria Playwright ${timestamp}`;
    const categoriaDescription = "Descrição Playwright";
    await inputName.fill(categoriaNome);
    await inputDesc.fill(categoriaDescription);

    // 5. Clica em Guardar (botão com id CreateSaveButton)
    await page.click("#CreateSaveButton");

    await expect(page.getByText("Fornecedor")).toBeVisible();
});
