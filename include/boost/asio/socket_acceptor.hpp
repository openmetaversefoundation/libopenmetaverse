//
// socket_acceptor.hpp
// ~~~~~~~~~~~~~~~~~~~
//
// Copyright (c) 2003-2005 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef BOOST_ASIO_SOCKET_ACCEPTOR_HPP
#define BOOST_ASIO_SOCKET_ACCEPTOR_HPP

#if defined(_MSC_VER) && (_MSC_VER >= 1200)
# pragma once
#endif // defined(_MSC_VER) && (_MSC_VER >= 1200)

#include <boost/asio/detail/push_options.hpp>

#include <boost/asio/basic_socket_acceptor.hpp>
#include <boost/asio/socket_acceptor_service.hpp>

namespace boost {
namespace asio {

/// Typedef for the typical usage of socket_acceptor.
typedef basic_socket_acceptor<socket_acceptor_service<> > socket_acceptor;

} // namespace asio
} // namespace boost

#include <boost/asio/detail/pop_options.hpp>

#endif // BOOST_ASIO_SOCKET_ACCEPTOR_HPP
