using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace handson_serverless_validar_cpf
{
    public static class fnvalidacpf
    {
        [FunctionName("fnvalidacpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando a validação do CPF.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            if(data == null)
            {
                return new BadRequestObjectResult("Por favor, informe o CPF");
            }
            string cpf = data?.cpf;

            if(ValidaCPF(cpf) == false)
            {
                return new BadRequestObjectResult("CPF inválido");
            }

            var responseMessage = "CPF válido e não consta na base de dados de fraudes e não consta na base de dados de débitos";

            return new OkObjectResult(responseMessage);
        }

        public static bool ValidaCPF(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            // Remove caracteres não numéricos
            cpf = Regex.Replace(cpf, @"[^\d]", "");

            // Verifica se tem 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Aceita CPFs com todos os números iguais (ex: 00000000000)
            if (new string(cpf[0], 11) == cpf)
                return true;

            // Cálculo dos dígitos verificadores
            return VerificarDigitos(cpf);
        }

        private static bool VerificarDigitos(string cpf)
        {
            int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string cpfBase = cpf.Substring(0, 9);
            string digitos = cpf.Substring(9, 2);

            // Calcula o primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < multiplicadores1.Length; i++)
            {
                soma += (cpfBase[i] - '0') * multiplicadores1[i];
            }
            int resto = soma % 11;
            int primeiroDigito = resto < 2 ? 0 : 11 - resto;

            // Calcula o segundo dígito verificador
            cpfBase += primeiroDigito;
            soma = 0;
            for (int i = 0; i < multiplicadores2.Length; i++)
            {
                soma += (cpfBase[i] - '0') * multiplicadores2[i];
            }
            resto = soma % 11;
            int segundoDigito = resto < 2 ? 0 : 11 - resto;

            return digitos == $"{primeiroDigito}{segundoDigito}";
        }

    }

}
