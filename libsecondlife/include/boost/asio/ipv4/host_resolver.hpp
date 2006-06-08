//
// host_resolver.hpp
// ~~~~~~~~~~~~~~~~~
//
// Copyright (c) 2003-2005 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef BOOST_ASIO_IPV4_HOST_RESOLVER_HPP
#define BOOST_ASIO_IPV4_HOST_RESOLVER_HPP

#if defined(_MSC_VER) && (_MSC_VER >= 1200)
# pragma once
#endif // defined(_MSC_VER) && (_MSC_VER >= 1200)

#include <boost/asio/detail/push_options.hpp>

#include <boost/asio/ipv4/basic_host_resolver.hpp>
#include <boost/asio/ipv4/host_resolver_service.hpp>
#include <boost/asio/ipv4/detail/host_resolver_service.hpp>

namespace boost {
namespace asio {
namespace ipv4 {

/// Typedef for the typical usage of host_resolver.
typedef basic_host_resolver<host_resolver_service<> > host_resolver;

} // namespace ipv4
} // namespace asio
} // namespace boost

#include <boost/asio/detail/pop_options.hpp>

#endif // BOOST_ASIO_IPV4_HOST_RESOLVER_HPP
