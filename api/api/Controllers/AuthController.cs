﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;

        public AuthController(IUserService userService, IJwtService jwtService, IEmailService emailService)
        {
            _userService = userService;
            _jwtService = jwtService;
            _emailService = emailService;
        }
        //Clients
        [HttpPost("sign-up")]
        public async Task<ActionResult<ServiceResponse<string?>>> SignUpClient(SignUpClientDTO request)
        {
            var registrationResponse = await _userService.SignUpClient(request);
            if (registrationResponse.Success)
            {
                var emailVerificationUrl = "http://localhost:3000/verify-email/" + registrationResponse.Data;
                var emailBody = "Votre compte a été crée avec succès, vous devez maintenant confirmé votre email pour pouvoir y accéder.<br /> Pour ce faire <br /><br /> <a href=\"#URL#\"> Cliquer ici</a>";
                emailBody = emailBody.Replace("#URL#", System.Text.Encodings.Web.HtmlEncoder.Default.Encode(emailVerificationUrl));
                var emailResponse = await _emailService.SendEmail(request.Email,  "Tiamshop, confirmation de la création de compte", emailBody);
                return emailResponse;
            }
            else
            {
                return registrationResponse;
            }
        }

        [HttpPost("sign-in-with-email")]
        public async Task<ActionResult<ServiceResponse<string?>>> LoginClientWithEmail(LoginClientWithEmailDTO request)
        {
            //We will try to log the user
            var login = await _userService.LoginClientWithEmail(request);

            //if the user is logged succesfully, we create a cookie that contains the token of the login
            if (login.Success && login.Data != null)
            {
                Response.Cookies.Append("clientLoginJwt", login.Data, new CookieOptions
                {
                    HttpOnly = true, //This means that the frontend can only get it but cannot access/modify it. 
                    Expires = request.RemenberMe ? DateTimeOffset.Now.AddYears(1) : DateTimeOffset.Now.AddMinutes(30)
                });
            }

            /*
             * Finally we will return a service response to inform if the login was successful or not(if not why)
             * We return a Service Response with a null data because we don't want the frontend to access the token.
            */
            return new ServiceResponse<string?>
            {
                Data = null,
                Success = login.Success,
                Message = login.Message,
            };
        }

        [HttpPost("sign-in-with-phone-number")]
        public async Task<ActionResult<ServiceResponse<string?>>> LoginClientWithPhoneNumber(LoginClientWithPhoneNumberDTO request)
        {
            // We will try to log the user
            var login = await _userService.LoginClientWithPhoneNumber(request);

            //if the user is logged succesfully, we create a cookie that contains the token of the login
            if (login.Success && login.Data != null)
            {
                Response.Cookies.Append("clientLoginJwt", login.Data, new CookieOptions
                {
                    HttpOnly = true, //This means that the frontend can only get it but cannot access/modify it. 
                    Expires = request.RemenberMe ? DateTimeOffset.Now.AddYears(1) : DateTimeOffset.Now.AddMinutes(30)
                });
            }

            /*
             * Finally we will return a service response to inform if the login was successful or not(if not why)
             * We return a Service Response with a null data because we don't want the frontend to access the token.
            */
            return new ServiceResponse<string?>
            {
                Data = null,
                Success = login.Success,
                Message = login.Message,
            };
        }

        [HttpGet("get-logged-client")]
        public async Task<ActionResult<ServiceResponse<GetUserDTO?>>> GetLoggedClient()
        {
            try
            {
                var clientLoginJwtFromCookies = Request.Cookies["clientLoginJwt"];

                var validatedClientLoginJwt = _jwtService.Verify(clientLoginJwtFromCookies);

                int userId = int.Parse(validatedClientLoginJwt.Issuer);

                var serviceResponse = await _userService.GetUserById(userId);

                if (serviceResponse.Data == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "INVALID_TOKEN";
                }

                return serviceResponse;
            }
            catch (Exception _)
            {
                return new ServiceResponse<GetUserDTO?>
                {
                    Data = null,
                    Success = false,
                    Message = "INVALID_TOKEN"
                };
            }

        }

        //Admins
        [HttpPost("admins/sign-in")]
        public async Task<ActionResult<ServiceResponse<string?>>> LoginAdmin(LoginAdminDTO request)
        {
            //We will try to log the user
            var login = await _userService.LoginAdmin(request);

            //if the user is logged succesfully, we create a cookie that contains the token of the login
            if (login.Success)
            {
                Response.Cookies.Append("adminLoginJWT", login.Data, new CookieOptions
                {
                    HttpOnly = true, //This means that the frontend can only get it but cannot access/modify it. 
                    Expires = DateTimeOffset.Now.AddMinutes(30),
                });
            }

            /*
             * Finally we will return a service response to inform if the login was successful or not(if not why)
             * We return a Service Response with a null data because we don't want the frontend to access the token.
            */
            return new ServiceResponse<string?>
            {
                Data = null,
                Success = login.Success,
                Message = login.Message,
            };
        }


        [HttpGet("admins/get-logged-admin")]
        public async Task<ActionResult<ServiceResponse<GetUserDTO?>>> GetLoggedAdmin()
        {
            try
            {
                var adminLoginJwtFromCookies = Request.Cookies["adminLoginJWT"];

                var validatedAdminLoginJwt = _jwtService.Verify(adminLoginJwtFromCookies);

                int userId = int.Parse(validatedAdminLoginJwt.Issuer);

                var serviceResponse = await _userService.GetUserById(userId);

                if (serviceResponse.Data == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "INVALID_TOKEN";
                }

                return serviceResponse;
            }
            catch (Exception _)
            {
                return new ServiceResponse<GetUserDTO?>
                {
                    Data = null,
                    Success = false,
                    Message = "INVALID_TOKEN"
                };
            }
        }



        //Admins & Clients
        [HttpPost("verify-email")]
        public async Task<ActionResult<ServiceResponse<string?>>> VerifyEmail(string token)
        {
            return await _userService.VerifyEmail(token);
        }

        [HttpPost("recover-password-with-email")]
        public async Task<ActionResult<ServiceResponse<string?>>> RecoverPasswordWithEmail(string email)
        {
            var response = await _userService.RecoverPasswordWithEmail(email);
            if (response.Success)
            {
                var emailVerificationUrl = "http://localhost:3000/reset-password/" + response.Data;
                var emailBody = "Pour créer un nouveau mot de passe<br /> <a href=\"#URL#\"> Cliquer ici</a> <br/><br/> ***NOTEZ-BIEN: VOUS N'AVEZ QUE 24 HEURES POUR CREER UN NOUVEAU MOT DE PASSE, SINON VOUS ALLEZ DEVOIR REPRENDRE LE PROCESSUS DE CHANGEMET DE MOT DE PASSE***";
                emailBody = emailBody.Replace("#URL#", System.Text.Encodings.Web.HtmlEncoder.Default.Encode(emailVerificationUrl));
                var emailResponse = await _emailService.SendEmail(email, "Tiamshop, création d'un nouveau mot de passe.", emailBody);
                return emailResponse;
            }
            else
            {
                return response;
            }
        }

        [HttpPost("recover-password-with-phone-number")]
        public async Task<ActionResult<ServiceResponse<string?>>> RecoverPasswordWithPhoneNumber(string phoneNumber)
        {
            var response = await _userService.RecoverPasswordWithPhoneNumber(phoneNumber);
            if (response.Success)
            {
                var getUser = await _userService.GetUserByPhoneNumber(phoneNumber);
                var user = getUser.Data;

                var emailVerificationUrl = "http://localhost:3000/reset-password/" + response.Data;
                var emailBody = "Pour créer un nouveau mot de passe<br /> <a href=\"#URL#\"> Cliquer ici</a> <br/><br/> ***NOTEZ-BIEN: VOUS N'AVEZ QUE 24 HEURES POUR CREER UN NOUVEAU MOT DE PASSE, SINON VOUS ALLEZ DEVOIR REPRENDRE LE PROCESSUS DE CHANGEMET DE MOT DE PASSE***";
                emailBody = emailBody.Replace("#URL#", System.Text.Encodings.Web.HtmlEncoder.Default.Encode(emailVerificationUrl));
                var emailResponse = await _emailService.SendEmail(user.Email, "Tiamshop, création d'un nouveau mot de passe.", emailBody);
                return emailResponse;
            }
            else
            {
                return response;
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ServiceResponse<string?>>> ResetPassword(ResetPasswordDTO request)
        {
            return await _userService.ResetPassword(request);
        }

        [HttpPost("send-verification-email")]
        public async Task<ActionResult<ServiceResponse<string?>>> SendVerificationEmail(SendVerificationEmailDTO request)
        {
            return Ok(new ServiceResponse<string>());
        }

    }
}
