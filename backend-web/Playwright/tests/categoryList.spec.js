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

test("Category – Ver lista de categorias", async ({ page }) => {
    await register(page);

    await expect(page.getByText("Sair")).toBeVisible();

    await page.getByRole("button", { name: "Sair" }).click(); // clicar no botao de sair

    await login(page);

    await page.goto("/category-list");
    await page.waitForLoadState("networkidle");

  // 3. Verifica título / texto principal
    await expect(page.getByText("Fornecedor")).toBeVisible();
});
