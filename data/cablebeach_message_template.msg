////////////////////////////////////////////////////////////////////////////////
// World Server
////////////////////////////////////////////////////////////////////////////////

&region_info =
{
	name: string,
	id: uuid,
	handle: binary,
	online: boolean,
	ip: binary,
	port: integer,
	map_texture_id: uuid,
	owner: uri,
	region_flags: integer,
	agent_count: integer,
	capabilities: { $: uri },
	water_height: real,
	default_position: [ real, real, real ],
	default_look_at: [ real, real, real ]
}

%%create_region

-> region_info

<-
{
	success: boolean,
	message: string
}

%%delete_region

->
{
	id: uuid
}

<-
{
	success: boolean,
	message: string
}

%%region_update

->
{
	region: region_info
}

<-
{
	success: boolean,
	message: string
}

%%fetch_region

&fetch_region_query = { id: uuid }
&fetch_region_query = { x: integer, y: integer, nearest: boolean }
&fetch_region_query = { name: string }

-> fetch_region_query

<- region_info

%%fetch_default_region

<- region_info

%%region_search

->
{
	query: string
}

<-
{
	regions: [ region_info, ... ]
}

%%get_region_count

<-
{
	count: integer
}

////////////////////////////////////////////////////////////////////////////////
// Identity
////////////////////////////////////////////////////////////////////////////////

// This message should be ignored if it originates from anywhere other than the
// identity server for the given identity

&identity_attribute = string
&identity_attribute = integer
&identity_attribute = date
&identity_attribute = uuid
&identity_attribute = uri
&identity_attribute = [ real, real, real ]

%%request_capabilities

->
{
    identity: uri,
    capabilities: [ uri, ... ]
}

<-
{
    capabilities: { $: uri }
}

%%authenticate_ll

->
{
	first: string,
	last: string,
	md5_pass: string,
	services: { $: [ string, ... ] }
}

<-
{
	success: boolean,
	message: string,
	identity: uri,
	attributes: { $: identity_attribute },
    services: { $: { $: uri } }
}

%%authenticate_openid

->
{
	identity: uri,
	# FIXME: Need whatever piece of data OpenID uses to confirm auth
	services: { $: [ string, ... ] }
}

<-
{
	success: boolean,
	message: string,
	identity: uri,
	attributes: { $: identity_attribute },
    services: { $: { $: uri } }
}

////////////////////////////////////////////////////////////////////////////////
// Assets
////////////////////////////////////////////////////////////////////////////////

&primitive =
{
	attachment_position: [ real, real, real ],
	attachment_rotation: [ real, real, real, real ],
	before_attachment_rotation:  [ real, real, real, real ],
	name: string,
	perms_base: integer,
	perms_owner: integer,
	perms_group: integer,
	perms_everyone: integer,
	perms_next_owner: integer,
	creator_identity: string,
	owner_identity: string,
	last_owner_identity: string,
	group_identity: string,
	last_attachment_point: integer,
	link_number: integer,
	local_id: integer,
	position: [ real, real, real ],
	rotation: [ real, real, real, real ],
	velocity: [ real, real, real ],
	angular_velocity: [ real, real, real ],
	scale: [ real, real, real ],
	sit_offset: [ real, real, real ],
	sit_rotation: [ real, real, real, real ],
	camera_eye_offset: [ real, real, real ],
	camera_at_offset: [ real, real, real ],
	state: integer,
	material: integer,
	sound_id: uuid,
	sound_gain: real,
	sound_radius: real,
	sound_flags: integer,
	text_color: [ real, real, real, real ],
	selected: boolean,
	selector_id: uuid,
	use_physics: boolean,
	phantom: boolean,
	remote_script_access_pin: integer,
	volume_detect: boolean,
	die_at_edge: boolean,
	return_at_edge: boolean,
	temporary: boolean,
	sandbox: boolean,
	rez_time: date,
	sale_price: integer,
	sale_type: integer,
	flexible:
	{
		softness: integer,
		gravity: real,
		drag: real,
		wind: real,
		tension: real,
		force: [ real, real, real ]
	},
	light:
	{
		color: [ real, real, real, real ],
		intensity: real,
		radius: real,
		cutoff: real,
		falloff: real
	},
	sculpt:
	{
		sculpt_texture: uuid,
		sculpt_type: integer
	},
	particles:
	{
		flags: integer,
		pattern: integer,
		max_age: real,
		start_age: real,
		inner_angle: real,
		outer_angle: real,
		burst_rate: real,
		burst_radius: real,
		burst_speed_min: real,
		burst_speed_max: real,
		burst_particle_count: integer,
		angular_velocity: [ real, real, real ],
		acceleration: [ real, real, real ],
		texture_id: uuid,
		target_id: uuid,
		data_flags: integer,
		particle_max_age: real,
		particle_start_color: [ real, real, real, real ],
		particle_end_color: [ real, real, real, real ],
		particle_start_scale_x: real,
		particle_start_scale_y: real,
		particle_end_scale_x: real,
		particle_end_scale_y: real
	},
	shape:
	{
		path_curve: integer,
		path_begin: real,
		path_end: real,
		path_scale_x: real,
		path_scale_y: real,
		path_shear_x: real,
		path_shear_y: real,
		path_twist: real,
		path_twist_begin: real,
		path_radius_offset: real,
		path_taper_x: real,
		path_taper_y: real,
		path_revolutions: real,
		path_skew: real,
		profile_curve: integer,
		profile_begin: real,
		profile_end: real,
		profile_hollow: real
	}
	faces:
	[
		{
			face_index: integer,
			image_id: uuid,
			color: [ real, real, real, real ],
			scale_s: real,
			scale_t: real,
			offset_s: real,
			offset_t: real,
			imagerot: real,
			bump: integer,
			fullbright: boolean,
			media_flags: integer
		},
		...
	],
	inventory:
	{
		serial: integer,
		items:
		[
			{
				id: uuid,
				parent_id: uuid,
				name: string,
				owner_identity: string,
				creator_identity: string,
				group_identity: string,
				asset_id: uuid,
				content_type: string,
				description: string,
				group_owned: boolean,
				perms_base: binary,
				perms_everyone: binary,
				perms_group: binary,
				perms_next: binary,
				perms_owner: binary,
				sale_price: integer,
				sale_type: integer,
				flags: integer,
				creation_date: integer,
				permission_granter: uuid,
				granted_permissions: integer
			},
			...
		]
	}
}

%%get_asset_metadata

&metadata = 
{
    id: uuid,
    name: string,
    description: string,
    creation_date: date,
    content_type: string,
    sha256: binary,
    temporary: boolean,
    methods: { $: uri }
}

&metadata = 
{
    id: uuid,
    name: string,
    description: string,
    creation_date: date,
    content_type: string,
    sha256: binary,
    temporary: boolean,
    methods: { $: uri }
    components: integer,
    layer_ends: [ integer ]
}

<- metadata

%%create_asset

&create =
{
    content_type: string,
    temporary: boolean,
    base64_data: string
}

&create =
{
    id: uuid,
    content_type: string,
    temporary: boolean,
    base64_data: string
}

-> create

<-
{
    id: uuid
}

%%delete_asset

->
{
    id: uuid
}

<-
{
    success: boolean,
    message: string
}

////////////////////////////////////////////////////////////////////////////////
// Inventory
////////////////////////////////////////////////////////////////////////////////

&inventory_object =
{
	id: uuid,
	parent_id: uuid,
	name: string,
	owner_id: uuid,
	asset_id: uuid,
	content_type: string,
	creator_id: uuid,
	group_id: uuid,
	description: string,
	group_owned: boolean,
	perms_base: binary,
	perms_everyone: binary,
	perms_group: binary,
	perms_next: binary,
	perms_owner: binary,
	sale_price: integer,
	sale_type: integer,
	flags: integer,
	creation_date: integer
}

&inventory_object =
{
	id: uuid,
	parent_id: uuid,
	name: string,
	owner_id: uuid,
	preferred_content_type: string,
	version: integer,
	children: [ inventory_object, ... ]
}

%%create_inventory

->
{
    identity: uri,
    name: string
}

<-
{
    root_folder_id: uuid
}

%%create_object

->
{
    identity: uri,
    object: inventory_object
}

<-
{
	success: boolean,
	message: string,
	object: inventory_object
}

%%fetch_object

->
{
	identity: uri,
	object_id: uuid
}

<-
{
	success: boolean,
	message: string,
	object: inventory_object
}

%%purge_folder

->
{
    identity: uri,
    folder_id: uuid
}

<-
{
	success: boolean,
	message: string
}

%%get_inventory_skeleton

->
{
    identity: uri
}

<-
{
    folders:
    [
        {
            name: string,
            parent_id: uuid,
            version: integer,
            preferred_content_type: string,
            folder_id: uuid
        },
        ...
    ]
}

%%get_active_gestures

->
{
    identity: uri
}

<-
{
    gestures:
    [
        {
            item_id: uuid,
            asset_id: uuid
        },
        ...
    ]
}

////////////////////////////////////////////////////////////////////////////////
// Simulators
////////////////////////////////////////////////////////////////////////////////

%%enable_client

->
{
	identity: uri,
    agent_id: uuid,
    session_id: uuid,
    secure_session_id: uuid,
    circuit_code: integer,
    region_handle: binary,
    child_agent: boolean,
    ip: binary,
    client_version: string,
    attributes: { },
    services: { $: { $: uri } },
    callback_uri: uri
}

<-
{
    success: boolean,
    message: string,
    seed_capability: uri,
}

%%enable_client_complete

->
{
    agent_id: uuid
}

%%close_agent_connection

->
{
    agent_id: uuid
}

%%neighbor_update

->
{
	neighbors: [ region_info, ... ]
}

%%child_agent_update

->
{
    agent_id: uuid,
    session_id: uuid,
    position: [ real, real, real ],
    velocity: [ real, real, real ],
    region_handle: binary,
    cam_position: [ real, real, real ],
    cam_at_axis: [ real, real, real ],
    cam_left_axis: [ real, real, real ],
    cam_up_axis: [ real, real, real ]
}

%%pass_object

->
{
    id: uuid
}

<-
{
    success: boolean,
    message: string
}

%%fetch_terrain

->
{
    blocks:
    [
        {
            x: integer,
            y: integer
        }
    ]
}

<-
{
    blocks:
    [
        {
            x: integer,
            y: integer,
            data: binary
        }
    ]
}
