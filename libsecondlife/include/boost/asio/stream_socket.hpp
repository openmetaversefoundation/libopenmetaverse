//
// stream_socket.hpp
// ~~~~~~~~~~~~~~~~~
//
// Copyright (c) 2003-2005 Christopher M. Kohlhoff (chris at kohlhoff dot com)
//
// Distributed under the Boost Software License, Version 1.0. (See accompanying
// file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//

#ifndef BOOST_ASIO_STREAM_SOCKET_HPP
#define BOOST_ASIO_STREAM_SOCKET_HPP

#if defined(_MSC_VER) && (_MSC_VER >= 1200)
# pragma once
#endif // defined(_MSC_VER) && (_MSC_VER >= 1200)

#include <boost/asio/detail/push_options.hpp>

#include <boost/asio/basic_stream_socket.hpp>
#include <boost/asio/stream_socket_service.hpp>

namespace boost {
namespace asio {

/// Typedef for the typical usage of stream_socket.
typedef basic_stream_socket<stream_socket_service<> > stream_socket;

} // namespace asio
} // namespace boost

#include <boost/asio/detail/pop_options.hpp>

#endif // BOOST_ASIO_STREAM_SOCKET_HPP
