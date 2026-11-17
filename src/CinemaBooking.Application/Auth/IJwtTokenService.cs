using System;
using System.Collections.Generic;
using CinemaBooking.Domain.Entities;

namespace CinemaBooking.Application.Auth;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(
        User user,
        IList<string> roles);
}